needs ../../utils/sdl.fs
needs ../../utils/files.fs
needs ../../utils/util.fs
needs ../../utils/shorts.fs
needs ../vm.fs

variable screen-cur-mem
0 screen-cur-mem !

variable screen-cur-font
0 screen-cur-font !
0 value screen-default-font

variable screen-cur-pallette
0 screen-cur-pallette !
0 value screen-default-pallette

\ 30 hz screen refresh
1000000 30 / constant screen-refresh-timeout
variable screen-last-refresh
0 screen-last-refresh !

\ blink every second
1000000 value screen-blink-timeout
variable screen-last-blink
0 screen-last-blink !
variable screen-blink
false screen-blink !

4 value screen-pixel-size

: pallette-entry ( r g b -- word )
		0xF and \ r g b
		swap 0xF and 4 lshift + \ r gb 
		swap 0xF and 8 lshift + \ rgb
;

: load-default-pallette
		screen-default-pallette free-addr-if-nonzero
		16 shorts allocate throw to screen-default-pallette
		\ black
		0x0000 screen-default-pallette 0x0 shorts + w!
		\ blue
		0x000a screen-default-pallette 0x1 shorts + w!
		\ green
		0x00a0 screen-default-pallette 0x2 shorts + w!
		\ teal
		0x00aa screen-default-pallette 0x3 shorts + w!
		\ red
		0x0a00 screen-default-pallette 0x4 shorts + w!
		\ magenta
		0x0a0a screen-default-pallette 0x5 shorts + w!
		\ orange
		0x0a50 screen-default-pallette 0x6 shorts + w!
		\ lgray
		0x0aaa screen-default-pallette 0x7 shorts + w!
		\ dgray
		0x0555 screen-default-pallette 0x8 shorts + w!
		\ lblue
		0x055f screen-default-pallette 0x9 shorts + w!
		\ lgreen
		0x05f5 screen-default-pallette 0xA shorts + w!
		\ lteal
		0x05ff screen-default-pallette 0xB shorts + w!
		\ lorange
		0x0f55 screen-default-pallette 0xC shorts + w!
		\ lmagenta
		0x0f5f screen-default-pallette 0xD shorts + w!
		\ yellow
		0x0ff5 screen-default-pallette 0xE shorts + w!
		\ white
		0x0fff screen-default-pallette 0xF shorts + w!
;

: load-default-font
		screen-default-font free-addr-if-nonzero
		256 shorts allocate throw to screen-default-font
		s" vm/hw/font.dfnt" screen-default-font 256 shorts read-sized-bin-file
		256 shorts <> if
				abort" Invalid font file (not 256 words)"
		then
;

: init-screen
		load-default-font
		load-default-pallette
		\ set screen to refresh (display) in 1 second
		1000000 utime d>s screen-last-refresh !
		false screen-blink !
		0 screen-last-blink !
		sdl-active? false = if
				start-sdl
		then
;

: screen-mem-map ( -- )
		REG_B reg-get 
		screen-cur-mem @ 0 = \ REG_B =0
		over 0 <> and if \ REG_B =0&B<>0
				init-screen
		then
		screen-cur-mem !
;

: update-blink ( cur-time[ns] -- )
		screen-last-blink @ screen-blink-timeout + > if
				." blink" cr
				screen-blink @ not screen-blink !
				utime d>s screen-last-blink !
		then
;

\ turns a 4bit color (0xA) into the 8-bit equiv (0xAA)
: num-4bit-to-8bit ( u -- u2 )
		dup 4 lshift +
;

: color-4bit-to-8bit ( r4 g4 b4 -- r8 g8 b8 )
		num-4bit-to-8bit
		swap num-4bit-to-8bit
		swap rot num-4bit-to-8bit
		-rot
;

\ convert lem coords to SDL screen
: screen->display ( x y -- x<screen> y<screen> )
		screen-pixel-size *
		swap screen-pixel-size *
		swap
;

\ takes in a screen x/y and 4-bit r g b values
: draw-screen-pixel ( x y r g b -- )
		color-4bit-to-8bit sdl-rgb \ x y rgb
		-rot \ rgb x y
		screen->display
		screen-pixel-size screen-pixel-size
		sdl-draw-block
;

: draw-character ( col row char -- )
;

: refresh-display ( cur-time[ns] -- )
		screen-last-refresh @ screen-refresh-timeout + > if
				\ draw the characters
				sdl-clear-black

				sdl-flip-screen
				utime d>s screen-last-refresh !
		then
;

: screen-updater
		\ do nothing if screen is disconnected
		screen-cur-mem @ 0 = if
				exit
		then
		utime d>s \ cur-time
		\ update blink if needed
		dup update-blink
		refresh-display
;
: screen-hw-int-handler
;
: screen-get-hw-info
		\ ID low/high word
		0xF615 REG_A reg-set
		0x7349 REG_B reg-set
		\ Version
		0x1802 REG_C reg-set
		\ Mfgr low/high word
		0x8B36 REG_X reg-set
		0x1C6C REG_Y reg-set
;

' screen-updater
cr dup ." Screen Updater: " . cr
' screen-hw-int-handler
dup ." Screen Int-handler: " . cr
' screen-get-hw-info
dup ." Screen Info: " . cr