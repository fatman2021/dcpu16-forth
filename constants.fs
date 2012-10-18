\ array indices for registers
#0 constant REG_A
#1 constant REG_B
#2 constant REG_C
#3 constant REG_X
#4 constant REG_Y
#5 constant REG_Z
#6 constant REG_I
#7 constant REG_J

\ vmloc types
#1  constant LOC_REG
#2  constant LOC_MEM
#3  constant LOC_REG_MEM
#4  constant LOC_REG_MEM_OFFSET
#5  constant LOC_LITERAL
#6  constant LOC_SP
#7  constant LOC_PC
#8  constant LOC_EX
#9  constant LOC_IA
#10 constant LOC_PUSHPOP
#11 constant LOC_PEEK
#12 constant LOC_PICK

\ Standard opcodes
0x00 constant OP_SPECIAL
0x01 constant OP_SET
0x02 constant OP_ADD
0x03 constant OP_SUB
0x04 constant OP_MUL
0x05 constant OP_MLI
0x06 constant OP_DIV
0x07 constant OP_DVI
0x08 constant OP_MOD
0x09 constant OP_MDI
0x0a constant OP_AND
0x0b constant OP_BOR
0x0c constant OP_XOR
0x0d constant OP_SHR
0x0e constant OP_ASR
0x0f constant OP_SHL
0x10 constant OP_IFB
0x11 constant OP_IFC
0x12 constant OP_IFE
0x13 constant OP_IFN
0x14 constant OP_IFG
0x15 constant OP_IFA
0x16 constant OP_IFL
0x17 constant OP_IFU
0x1a constant OP_ADX
0x1b constant OP_SBX
0x1e constant OP_STI
0x1f constant OP_STD

\ Special opcodes
0x01 constant OP_JSR
0x08 constant OP_INT
0x09 constant OP_IAG
0x0a constant OP_IAS
0x0b constant OP_RFI
0x0c constant OP_IAQ
0x10 constant OP_HWN
0x11 constant OP_HWQ
0x12 constant OP_HWI

