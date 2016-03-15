using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Map27Decode
{
    // Code taken from MAP27 API appendix A1
    // Generator polynomial G(x) = x16 + x15 + x2 + 1. 
    public class Crc16
    {
        UInt16 CRC16 = 0xA001;	/* CRC 16 constant */
        UInt16[] mtab = new UInt16[256];	/* modification table */

        public Crc16()
        {
            create_table(mtab);
        }

        public UInt16 CalculateCrc(byte [] input)
        {
            return fcs_calc(mtab, input, (UInt16) input.Length);
        }
        
        /************************************************************
            Function: create_table	produce the look-up table
            Input:    *mtab	pointer to modification table
            Output:
            Note:     CRC16 is used
        ************************************************************/
        void create_table(UInt16[] mtab)
        {
            UInt16[] btab = new UInt16[8];	/* table btab */
            UInt16 i, j; 	/* loop parameters */
            UInt16 q;	/* calculation register */
            UInt16 shreg;	/* shift-register */
            UInt16 carry, bit;	/* bit parameters */

            /************************************************************
                Calculate the table btab:
            ************************************************************/
            carry = 1;	/* carry flag set to one */
            shreg = 0;	/* shreg initialised with 0 */
            for (i = 0; i < 8; i++)
            {
                if (0 != carry)
                    shreg ^= CRC16;

                btab[i] = (UInt16)((shreg << 8) | (shreg >> 8));	/* swap bytes */
                carry = (UInt16)(shreg & 1);
                shreg >>= 1;
            }

            /************************************************************
                Calculate the modification table mtab:
            ************************************************************/
            int mtabIndex = 0;
            for (i = 0; i < 256; i++)
            {
                q = 0;
                bit = 0x80;
                for (j = 0; j < 8; j++)
                {
                    if (0 != (bit & i))
                        q ^= btab[j];

                    bit >>= 1;
                }
                mtab[mtabIndex++] = q;
            }
        }

        /************************************************************
            Function:	fcs_calc	calculates the FCS sequence
            Input:	*mtab	pointer to modification table
    	        *buff	pointer to character buffer
    	        len	length of character string
            Output: 	fcs	frame check sequence
            Note:	fcs is initialised with all ones
        ************************************************************/
        UInt16 fcs_calc(UInt16[] mtab, byte[] buff, UInt16 len)
        {
            UInt16 fcs; 	/* frame check sequence */
            UInt16 q; 	/* calculation register */
            int bufIndex = 0;

            fcs = 0xffff; 	/* fcs initialised with all ones */
            while (0 != len--)
            {
                q = mtab[ + (buff[bufIndex++] ^ (fcs >> 8))];
                fcs = (UInt16)(((q & 0xff00) ^ (fcs << 8)) | (q & 0x00ff));
            }
            return (UInt16)(fcs ^ 0xffff); /* return the fcs ones complement */
        }

    }
}
