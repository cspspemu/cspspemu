using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Crypto
{
	unsafe public partial class Crypto
	{

		/// <summary>
		/// Define the circular shift macro
		/// </summary>
		/// <param name="bits"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		static protected uint SHA1CircularShift(int bits, uint word) {
			return ((((word) << (bits)) & 0xFFFFFFFF) | ((word) >> (32-(bits))));
		}

		/*  
		 *  SHA1Reset
		 *
		 *  Description:
		 *      This function will initialize the SHA1Context in preparation
		 *      for computing a new message digest.
		 *
		 *  Parameters:
		 *      context: [in/out]
		 *          The context to reset.
		 *
		 *  Returns:
		 *      Nothing.
		 *
		 *  Comments:
		 *
		 */
		public static void SHA1Reset(SHA1Context *context)
		{
			context->Length_Low             = 0;
			context->Length_High            = 0;
			context->Message_Block_Index    = 0;

			context->Message_Digest[0]      = 0x67452301;
			context->Message_Digest[1]      = 0xEFCDAB89;
			context->Message_Digest[2]      = 0x98BADCFE;
			context->Message_Digest[3]      = 0x10325476;
			context->Message_Digest[4]      = 0xC3D2E1F0;

			context->Computed   = false;
			context->Corrupted  = false;
		}

		/*  
		 *  SHA1Result
		 *
		 *  Description:
		 *      This function will return the 160-bit message digest into the
		 *      Message_Digest array within the SHA1Context provided
		 *
		 *  Parameters:
		 *      context: [in/out]
		 *          The context to use to calculate the SHA-1 hash.
		 *
		 *  Returns:
		 *      1 if successful, 0 if it failed.
		 *
		 *  Comments:
		 *
		 */
		public static int SHA1Result(SHA1Context *context)
		{

			if (context->Corrupted)
			{
				return 0;
			}

			if (!context->Computed)
			{
				SHA1PadMessage(context);
				context->Computed = true;
			}

			return 1;
		}

		/*  
		 *  SHA1Input
		 *
		 *  Description:
		 *      This function accepts an array of octets as the next portion of
		 *      the message.
		 *
		 *  Parameters:
		 *      context: [in/out]
		 *          The SHA-1 context to update
		 *      message_array: [in]
		 *          An array of characters representing the next portion of the
		 *          message.
		 *      length: [in]
		 *          The length of the message in message_array
		 *
		 *  Returns:
		 *      Nothing.
		 *
		 *  Comments:
		 *
		 */
		public static void SHA1Input(     SHA1Context         *context,
							byte *message_array,
							int            length)
		{
			if (length == 0)
			{
				return;
			}

			if (context->Computed || context->Corrupted)
			{
				context->Corrupted = true;
				return;
			}

			while(length-- > 0 && !context->Corrupted)
			{
				context->Message_Block[context->Message_Block_Index++] =
														(byte)(*message_array & 0xFF);

				context->Length_Low += 8;
				/* Force it to 32 bits */
				context->Length_Low &= 0xFFFFFFFF;
				if (context->Length_Low == 0)
				{
					context->Length_High++;
					/* Force it to 32 bits */
					context->Length_High &= 0xFFFFFFFF;
					if (context->Length_High == 0)
					{
						/* Message is too long */
						context->Corrupted = true;
					}
				}

				if (context->Message_Block_Index == 64)
				{
					SHA1ProcessMessageBlock(context);
				}

				message_array++;
			}
		}

		/*  
		 *  SHA1ProcessMessageBlock
		 *
		 *  Description:
		 *      This function will process the next 512 bits of the message
		 *      stored in the Message_Block array.
		 *
		 *  Parameters:
		 *      None.
		 *
		 *  Returns:
		 *      Nothing.
		 *
		 *  Comments:
		 *      Many of the variable names in the SHAContext, especially the
		 *      single character names, were used because those were the names
		 *      used in the publication.
		 *         
		 *
		 */
		public static void SHA1ProcessMessageBlock(SHA1Context *context)
		{
			var K = new uint[]           /* Constants defined in SHA-1   */      
			{
				0x5A827999,
				0x6ED9EBA1,
				0x8F1BBCDC,
				0xCA62C1D6
			};
			int         t;                  /* Loop counter                 */
			uint    temp;               /* Temporary word value         */
			var    W = new uint[80];              /* Word sequence                */
			uint    A, B, C, D, E;      /* Word buffers                 */

			/*
			 *  Initialize the first 16 words in the array W
			 */
			for(t = 0; t < 16; t++)
			{
				W[t] = ((uint)context->Message_Block[t * 4]) << 24;
				W[t] |= ((uint)context->Message_Block[t * 4 + 1]) << 16;
				W[t] |= ((uint)context->Message_Block[t * 4 + 2]) << 8;
				W[t] |= ((uint)context->Message_Block[t * 4 + 3]);
			}

			for(t = 16; t < 80; t++)
			{
			   W[t] = SHA1CircularShift(1,W[t-3] ^ W[t-8] ^ W[t-14] ^ W[t-16]);
			}

			A = context->Message_Digest[0];
			B = context->Message_Digest[1];
			C = context->Message_Digest[2];
			D = context->Message_Digest[3];
			E = context->Message_Digest[4];

			for(t = 0; t < 20; t++)
			{
				temp =  SHA1CircularShift(5,A) +
						((B & C) | ((~B) & D)) + E + W[t] + K[0];
				temp &= 0xFFFFFFFF;
				E = D;
				D = C;
				C = SHA1CircularShift(30,B);
				B = A;
				A = temp;
			}

			for(t = 20; t < 40; t++)
			{
				temp = SHA1CircularShift(5,A) + (B ^ C ^ D) + E + W[t] + K[1];
				temp &= 0xFFFFFFFF;
				E = D;
				D = C;
				C = SHA1CircularShift(30,B);
				B = A;
				A = temp;
			}

			for(t = 40; t < 60; t++)
			{
				temp = SHA1CircularShift(5,A) +
					   ((B & C) | (B & D) | (C & D)) + E + W[t] + K[2];
				temp &= 0xFFFFFFFF;
				E = D;
				D = C;
				C = SHA1CircularShift(30,B);
				B = A;
				A = temp;
			}

			for(t = 60; t < 80; t++)
			{
				temp = SHA1CircularShift(5,A) + (B ^ C ^ D) + E + W[t] + K[3];
				temp &= 0xFFFFFFFF;
				E = D;
				D = C;
				C = SHA1CircularShift(30,B);
				B = A;
				A = temp;
			}

			context->Message_Digest[0] = (context->Message_Digest[0] + A) & 0xFFFFFFFF;
			context->Message_Digest[1] = (context->Message_Digest[1] + B) & 0xFFFFFFFF;
			context->Message_Digest[2] = (context->Message_Digest[2] + C) & 0xFFFFFFFF;
			context->Message_Digest[3] = (context->Message_Digest[3] + D) & 0xFFFFFFFF;
			context->Message_Digest[4] = (context->Message_Digest[4] + E) & 0xFFFFFFFF;

			context->Message_Block_Index = 0;
		}

		/*  
		 *  SHA1PadMessage
		 *
		 *  Description:
		 *      According to the standard, the message must be padded to an even
		 *      512 bits.  The first padding bit must be a '1'.  The last 64
		 *      bits represent the length of the original message.  All bits in
		 *      between should be 0.  This function will pad the message
		 *      according to those rules by filling the Message_Block array
		 *      accordingly.  It will also call SHA1ProcessMessageBlock()
		 *      appropriately.  When it returns, it can be assumed that the
		 *      message digest has been computed.
		 *
		 *  Parameters:
		 *      context: [in/out]
		 *          The context to pad
		 *
		 *  Returns:
		 *      Nothing.
		 *
		 *  Comments:
		 *
		 */
		static public void SHA1PadMessage(SHA1Context *context)
		{
			/*
			 *  Check to see if the current message block is too small to hold
			 *  the initial padding bits and length.  If so, we will pad the
			 *  block, process it, and then continue padding into a second
			 *  block.
			 */
			if (context->Message_Block_Index > 55)
			{
				context->Message_Block[context->Message_Block_Index++] = 0x80;
				while(context->Message_Block_Index < 64)
				{
					context->Message_Block[context->Message_Block_Index++] = 0;
				}

				SHA1ProcessMessageBlock(context);

				while(context->Message_Block_Index < 56)
				{
					context->Message_Block[context->Message_Block_Index++] = 0;
				}
			}
			else
			{
				context->Message_Block[context->Message_Block_Index++] = 0x80;
				while(context->Message_Block_Index < 56)
				{
					context->Message_Block[context->Message_Block_Index++] = 0;
				}
			}

			/*
			 *  Store the message length as the last 8 octets
			 */
			context->Message_Block[56] = (byte)((context->Length_High >> 24) & 0xFF);
			context->Message_Block[57] = (byte)((context->Length_High >> 16) & 0xFF);
			context->Message_Block[58] = (byte)((context->Length_High >> 8) & 0xFF);
			context->Message_Block[59] = (byte)((context->Length_High) & 0xFF);
			context->Message_Block[60] = (byte)((context->Length_Low >> 24) & 0xFF);
			context->Message_Block[61] = (byte)((context->Length_Low >> 16) & 0xFF);
			context->Message_Block[62] = (byte)((context->Length_Low >> 8) & 0xFF);
			context->Message_Block[63] = (byte)((context->Length_Low) & 0xFF);

			SHA1ProcessMessageBlock(context);
		}

	}
}
