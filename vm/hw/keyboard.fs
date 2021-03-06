needs ../../utils/sdl.fs
needs ../../utils/util.fs
needs ../../utils/shorts.fs
needs ../vm.fs

sdl-event% %alloc value evt

256 constant key-buffer-size
\ ring buffer
key-buffer-size allocate throw value key-buffer
variable key-buffer-start
0 key-buffer-start !
variable key-buffer-end
0 key-buffer-end !

0 value keyboard-int-msg

: key-buffer-clear
		key-buffer-end @
		key-buffer-start !
;
: key-buffer-empty? ( -- t/f )
		key-buffer-start @
		key-buffer-end @ =
;
: key-buffer-full? ( -- t/f )
		key-buffer-start
		key-buffer-end 1+ key-buffer-size mod
		=
;

: key-buffer-add ( c -- )
		." Adding key: " dup emit cr
		key-buffer-full? not if
				key-buffer key-buffer-end @ + c!
				1 key-buffer-end @ +
				key-buffer-size mod key-buffer-end !
		else
				." Keyboard buffer full!" cr
		then
;

: key-buffer-get ( -- c )
		key-buffer-empty? not if
				key-buffer key-buffer-start @ + c@
				1 key-buffer-start @ +
				key-buffer-size mod key-buffer-start !
		else
				\ return 0 if empty
				0
		then
;

\ if a key isn't between 0x20 and 0x7f, converts SDLK_<key>
\   into the keyboard spec's keys
: sdlk->key ( c -- c' )
		dup 0x7F 0x20 inside? not if
				case
						SDLK_BACKSPACE of 0x10 endof
						SDLK_RETURN of 0x11 endof
						SDLK_INSERT of 0x12 endof
						SDLK_DELETE of 0x13 endof
						SDLK_UP of 0x80 endof
						SDLK_DOWN of 0x81 endof
						SDLK_LEFT of 0x82 endof
						SDLK_RIGHT of 0x83 endof
						SDLK_LSHIFT of 0x90 endof
						SDLK_RSHIFT of 0x90 endof
						SDLK_LCTRL of 0x91 endof
						SDLK_RCTRL of 0x91 endof
						0x0
				endcase
		then
		." Key: " dup hex . decimal cr
;

: key->sdlk ( c -- c' )
		dup 0x7F 0x20 inside? not if
				case
						0x10 of SDLK_BACKSPACE endof
						0x11 of SDLK_RETURN endof
						0x12 of SDLK_INSERT endof
						0x13 of SDLK_DELETE endof
						0x80 of SDLK_UP endof
						0x81 of SDLK_DOWN endof
						0x82 of SDLK_LEFT endof
						0x83 of SDLK_RIGHT endof
						0x90 of SDLK_RSHIFT endof
						0x91 of SDLK_RCTRL endof
						0x00
				endcase
		then
;

: keyboard-send-int ( -- )
		keyboard-int-msg 0 over <> if
				sw-interrupt
		else
				drop
		then
;

: keyboard-updater
		sdl-active? not if
				start-sdl
		then
		evt sdl-poll-event if
				evt sdl-event-type c@
				sdl-event-key-down = if
						evt sdl-event-key sdl-keyboard-event-keysym int@ 
						sdlk->key
						dup if
								key-buffer-add
								keyboard-send-int
						else
								drop
						then
				then
		then
;

variable sdl-numkeys
0 sdl-numkeys !
: key-checker ( c -- t/f )
		sdl-numkeys @ 0 = if
				sdl-numkeys sdl-get-key-state \ c keys
				sdl-numkeys !
		then
		sdl-numkeys + c@
;

: keyboard-int-clear
		key-buffer-clear
;
: keyboard-int-get
		key-buffer-get
		REG_C reg-set
;
: keyboard-int-check
		REG_B reg-get
		key-checker if
				0x1 REG_C reg-set
		else
				0x0 REG_C reg-set
		then
;
: keyboard-int-set-msg
		REG_B reg-get to keyboard-int-msg
;

create keyboard-int-handlers
' keyboard-int-clear ,
' keyboard-int-get ,
' keyboard-int-check ,
' keyboard-int-set-msg ,

: keyboard-hw-int-handler
		." Keyboard HW Int" cr
		REG_A reg-get
		cells keyboard-int-handlers + @
		execute
;

: keyboard-get-hw-info
		\ ID low/high word
		0x7406 REG_A reg-set
		0x30CF REG_B reg-set
		\ Version
		0x0001 REG_C reg-set
		\ Mfgr low/high word
		0x1337 REG_X reg-set
		0xB00B REG_Y reg-set
;

' keyboard-updater
cr dup ." Keyboard Updater: " . cr
' keyboard-hw-int-handler
dup ." Keyboard Int-handler: " . cr
' keyboard-get-hw-info
dup ." Keyboard Info: " . cr
