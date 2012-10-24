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
						true over codelistentry-aval @ get-tokenval-size
						swap false swap codelistentry-bval @ get-tokenval-size
						+ 1+
				endof
				codelistentry-type_special-op of
						true swap codelistentry-aval @ get-tokenval-size
						1+
				endof
				codelistentry-type_label of
						drop 0
				endof
				codelistentry-type_data of
						\ TODO
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
		case dup codelistentry-type
				CODELISTENTRY-TYPE_OP of
						\ free a, b
						dup codelistentry-aval free
						dup codelistentry-bval free
				endof
				CODELISTENTRY-TYPE_SPECIAL-OP of
						\ free a
						dup codelistentry-aval free
				endof
				CODELISTENTRY-TYPE_DATA of
						\ free data
						dup codelistentry-data free
				endof
		endcase
;

( codelistentry -- word [word] [word] count )
\ count is 0, 1, or 2 for number of extra words
\ words are encoded word, extra word1, extra word2
: codelistentry-encode-OP
		\ encode op
		dup codelistentry-aval @
		\ encode a
		encode-tokenval
		\ encode b
		\ combine first word
;
: codelistentry-encode-SPECIAL-OP
		\ encode a
		\ encode op
		\ combine
;
: codelistentry-encode-LABEL
;
: codelistentry-encode-DATA
;
create codelistentry-encoders
' codelistentry-encode-OP ,
' codelistentry-encode-SPECIAL-OP ,
' codelistentry-encode-LABEL ,
' codelistentry-encode-DATA ,

