needs tokenval.fs

0x0 constant CODELISTENTRY-TYPE_OP
0x1 constant CODELISTENTRY-TYPE_SPECIAL-OP
0x2 constant CODELISTENTRY-TYPE_LABEL
0x3 constant CODELISTENTRY-TYPE_DATA

struct
		cell% field codelistentry-type
		cell% field codelistentry-op
		cell% field codelistentry-bval
		cell% field codelistentry-aval
		\ pointer to entry in code-labels list
		cell% field codelistentry-label
		\ counted array of data to copy over verbatim
		cell% field codelistentry-data
		\ how many words this code entry is into the code stream
		cell% field codelistentry-codeloc
		cell% field codelistentry-next
end-struct codelistentry

\ gets the size of a code list entry, for ops, size is 1-3, for labels, 0
\   for data, 0+
: get-codelistentry-size ( codelistentry -- size )
		case dup codelistentry-type @
				codelistentry-type_op of
						." Standard op codelistentry-size" cr
						true over codelistentry-aval @ get-tokenval-size
						swap false swap codelistentry-bval @ get-tokenval-size
						+ 1+
				endof
				codelistentry-type_special-op of
						." Special op codelistentry-size" cr
						true swap codelistentry-aval @ get-tokenval-size
						1+
				endof
				codelistentry-type_label of
						." Label codelistentry-size" cr
						drop 0
				endof
				codelistentry-type_data of
						." Data codelistentry-size" cr
						codelistentry-data @ w@ \ get first entry of array
				endof
		endcase
;

: get-codelistentry ( -- loc )
		codelistentry %alloc
		dup codelistentry swap drop erase
;

: dump-codelistentry ( loc -- loc )
		dup codelistentry swap drop dump
;

: dump-codelistentry-op ( loc -- loc )
		cr ." Code List Entry: " cr
		dump-codelistentry cr
		." B: " cr
		dup codelistentry-bval @ dump-tokenval drop cr
		." A: " cr
		dup codelistentry-aval @ dump-tokenval drop cr
;

\ free the memory allocated for a codelistentry
: empty-codelistentry ( entry -- )
		case dup codelistentry-type @
				CODELISTENTRY-TYPE_OP of
						\ free a, b
						dup codelistentry-aval @ free throw
						codelistentry-bval @ free throw
				endof
				CODELISTENTRY-TYPE_SPECIAL-OP of
						\ free a
						codelistentry-aval @ free throw
				endof
				CODELISTENTRY-TYPE_DATA of
						\ free data
						codelistentry-data @ free throw
				endof
				CODELISTENTRY-TYPE_LABEL of
						\ get rid of entry
						drop
				endof
		endcase
;

variable codelistentry-encode-accum
variable codelistentry-encode-size

( codelistentry -- [wordb] [worda] baseword count )
\ count is number of extra words ( past the first one )
\ words are encoded word, extra word1, extra word2
: codelistentry-encode-OP
		." encode op" cr
		>r \ store codelistentry on return stack
		\ set up local vars
		0 codelistentry-encode-accum !
		0 codelistentry-encode-size !

		\ encode op
		r@ codelistentry-op w@ codelistentry-encode-accum !
		\ encode b
		r@ codelistentry-bval @ \ bval
		false swap encode-tokenval \ bword [bword1] count
		1 = if \ .. bword [bword1]
				1 codelistentry-encode-size +!
				swap
		then \ .. bword
		#5 lshift codelistentry-encode-accum @ +
		0xFFFF and codelistentry-encode-accum !

		\ encode a
		r@ codelistentry-aval @ \ [bword] aval
		true swap encode-tokenval \ [bword] aword [aword1] count
		1 = if \ [bword] aword [aword1]
				1 codelistentry-encode-size +!
				swap
		then \ [bword] [aword1] aword
		#10 lshift codelistentry-encode-accum @ +
		0xFFFF and
		\ [bword] [aword] baseword
		codelistentry-encode-size @
		\ get rid of stored codelistentry
		r> drop
;
: codelistentry-encode-SPECIAL-OP
		." Encode special op" cr
		>r \ store codelistentry on return stack
		\ set up vars
		0 codelistentry-encode-accum !
		0 codelistentry-encode-size !

		\ encode op
		r@ codelistentry-op w@
		#5 lshift
		codelistentry-encode-accum !
		\ encode a
		r@ codelistentry-aval @
		true swap encode-tokenval \ aword [aword1] count
		1 = if \ aword aword1
				1 codelistentry-encode-size +!
				swap \ aword1 aword
		then \ ..aword
		#10 lshift codelistentry-encode-accum @ +
		0xFFFF and \ [aword1] entry
		codelistentry-encode-size @
		r> drop
;
: codelistentry-encode-LABEL
		drop -1
;
: codelistentry-encode-DATA
		." Encode data" cr
		codelistentry-data @ \ data
		\ need push items onto stack in reverse
		dup w@ \ data count
		dup >r
		begin
				2dup shorts + w@ \ data+1 count word
				-rot \ word data+1 count
				1-
		dup 0 = until
		2drop \ word...
		r> 1- \ word... count-1
;
create codelistentry-encoders
' codelistentry-encode-OP ,
' codelistentry-encode-SPECIAL-OP ,
' codelistentry-encode-LABEL ,
' codelistentry-encode-DATA ,

( codelistentry -- word [word] [word] count )
\ if -1 is returned, don't output any code
: encode-codelistentry
		codelistentry-encoders
		over codelistentry-type @
		cells + @
		execute
;

