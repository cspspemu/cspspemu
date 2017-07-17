using CSharpUtils;

namespace CSPspEmu.Core.Components.Crypto
{
    public unsafe partial class Crypto
    {
        //#define GETuint(pt) (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]))
        //#define PUTuint(ct, st) { (ct)[0] = (u8)((st) >> 24); (ct)[1] = (u8)((st) >> 16); (ct)[2] = (u8)((st) >>  8); (ct)[3] = (u8)(st); }
        //private static uint GETuint(byte[] pt) => (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]));

        private static uint GeTuint(byte* pt) =>
            (((uint) (pt)[0] << 24) ^ ((uint) (pt)[1] << 16) ^ ((uint) (pt)[2] << 8) ^ (pt)[3]);

        //memcpy(block_buff, src, 16);
        public static void Memcpy(byte* dst, byte* src, int count) => PointerUtils.Memcpy(dst, src, count);

        public static int Memcmp(byte* str1, byte* str2, int count)
        {
            for (var n = 0; n < count; n++)
            {
                var c1 = str1[n];
                var c2 = str2[n];
                if (c1 > c2) return -1;
                if (c1 < c2) return +1;
            }
            return 0;
        }


        public static void Memcpy(void* dst, void* src, int count) => Memcpy((byte*) dst, (byte*) src, count);

        //private static void PUTuint(byte[] ct, uint st) {
        //	(ct)[0] = (byte)((st) >> 24);
        //	(ct)[1] = (byte)((st) >> 16);
        //	(ct)[2] = (byte)((st) >>  8);
        //	(ct)[3] = (byte)(st);
        //}

        private static void PuTuint(byte* ct, uint st)
        {
            (ct)[0] = (byte) ((st) >> 24);
            (ct)[1] = (byte) ((st) >> 16);
            (ct)[2] = (byte) ((st) >> 8);
            (ct)[3] = (byte) (st);
        }

        /// <summary>
        /// Expand the cipher key into the encryption key schedule.
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="cipherKey"></param>
        /// <param name="keyBits"></param>
        /// <returns>the number of rounds for the given cipher key size.</returns>
        public static int RijndaelKeySetupEnc(uint* rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
        {
            var i = 0;
            uint temp;

            rk[0] = GeTuint(cipherKey);
            rk[1] = GeTuint(cipherKey + 4);
            rk[2] = GeTuint(cipherKey + 8);
            rk[3] = GeTuint(cipherKey + 12);
            if (keyBits == 128)
            {
                for (;;)
                {
                    temp = rk[3];
                    rk[4] = rk[0] ^
                            (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
                            (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
                            (Te4[(temp) & 0xff] & 0x0000ff00) ^
                            (Te4[(temp >> 24)] & 0x000000ff) ^
                            Rcon[i];
                    rk[5] = rk[1] ^ rk[4];
                    rk[6] = rk[2] ^ rk[5];
                    rk[7] = rk[3] ^ rk[6];
                    if (++i == 10)
                    {
                        return 10;
                    }
                    rk += 4;
                }
            }
            rk[4] = GeTuint(cipherKey + 16);
            rk[5] = GeTuint(cipherKey + 20);
            if (keyBits == 192)
            {
                for (;;)
                {
                    temp = rk[5];
                    rk[6] = rk[0] ^
                            (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
                            (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
                            (Te4[(temp) & 0xff] & 0x0000ff00) ^
                            (Te4[(temp >> 24)] & 0x000000ff) ^
                            Rcon[i];
                    rk[7] = rk[1] ^ rk[6];
                    rk[8] = rk[2] ^ rk[7];
                    rk[9] = rk[3] ^ rk[8];
                    if (++i == 8)
                    {
                        return 12;
                    }
                    rk[10] = rk[4] ^ rk[9];
                    rk[11] = rk[5] ^ rk[10];
                    rk += 6;
                }
            }
            rk[6] = GeTuint(cipherKey + 24);
            rk[7] = GeTuint(cipherKey + 28);
            if (keyBits != 256) return 0;
            for (;;)
            {
                temp = rk[7];
                rk[8] = rk[0] ^
                        (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
                        (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
                        (Te4[(temp) & 0xff] & 0x0000ff00) ^
                        (Te4[(temp >> 24)] & 0x000000ff) ^
                        Rcon[i];
                rk[9] = rk[1] ^ rk[8];
                rk[10] = rk[2] ^ rk[9];
                rk[11] = rk[3] ^ rk[10];
                if (++i == 7)
                {
                    return 14;
                }
                temp = rk[11];
                rk[12] = rk[4] ^
                         (Te4[(temp >> 24)] & 0xff000000) ^
                         (Te4[(temp >> 16) & 0xff] & 0x00ff0000) ^
                         (Te4[(temp >> 8) & 0xff] & 0x0000ff00) ^
                         (Te4[(temp) & 0xff] & 0x000000ff);
                rk[13] = rk[5] ^ rk[12];
                rk[14] = rk[6] ^ rk[13];
                rk[15] = rk[7] ^ rk[14];
                rk += 8;
            }
        }

        /// <summary>
        /// Expand the cipher key into the decryption key schedule.
        /// </summary>
        /// <param name="rk"></param>
        /// <param name="cipherKey"></param>
        /// <param name="keyBits"></param>
        /// <returns>the number of rounds for the given cipher key size.</returns>
        public static int RijndaelKeySetupDec(uint*rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
        {
            int i, j;

            /* expand the cipher key: */
            var nr = RijndaelKeySetupEnc(rk, cipherKey, keyBits);

            /* invert the order of the round keys: */
            for (i = 0, j = 4 * nr; i < j; i += 4, j -= 4)
            {
                var temp = rk[i];
                rk[i] = rk[j];
                rk[j] = temp;
                temp = rk[i + 1];
                rk[i + 1] = rk[j + 1];
                rk[j + 1] = temp;
                temp = rk[i + 2];
                rk[i + 2] = rk[j + 2];
                rk[j + 2] = temp;
                temp = rk[i + 3];
                rk[i + 3] = rk[j + 3];
                rk[j + 3] = temp;
            }
            /* apply the inverse MixColumn transform to all round keys but the first and the last: */
            for (i = 1; i < nr; i++)
            {
                rk += 4;
                rk[0] =
                    Td0[Te4[(rk[0] >> 24)] & 0xff] ^
                    Td1[Te4[(rk[0] >> 16) & 0xff] & 0xff] ^
                    Td2[Te4[(rk[0] >> 8) & 0xff] & 0xff] ^
                    Td3[Te4[(rk[0]) & 0xff] & 0xff];
                rk[1] =
                    Td0[Te4[(rk[1] >> 24)] & 0xff] ^
                    Td1[Te4[(rk[1] >> 16) & 0xff] & 0xff] ^
                    Td2[Te4[(rk[1] >> 8) & 0xff] & 0xff] ^
                    Td3[Te4[(rk[1]) & 0xff] & 0xff];
                rk[2] =
                    Td0[Te4[(rk[2] >> 24)] & 0xff] ^
                    Td1[Te4[(rk[2] >> 16) & 0xff] & 0xff] ^
                    Td2[Te4[(rk[2] >> 8) & 0xff] & 0xff] ^
                    Td3[Te4[(rk[2]) & 0xff] & 0xff];
                rk[3] =
                    Td0[Te4[(rk[3] >> 24)] & 0xff] ^
                    Td1[Te4[(rk[3] >> 16) & 0xff] & 0xff] ^
                    Td2[Te4[(rk[3] >> 8) & 0xff] & 0xff] ^
                    Td3[Te4[(rk[3]) & 0xff] & 0xff];
            }
            return nr;
        }

        public static void RijndaelEncrypt(uint*rk /*4*(Nr + 1)*/, int nr, byte* pt /*16*/, byte* ct /*[16]*/)
        {
            uint t0, t1, t2, t3;

            // map byte array block to cipher state
            // and add initial round key:
            var s0 = GeTuint(pt) ^ rk[0];
            var s1 = GeTuint(pt + 4) ^ rk[1];
            var s2 = GeTuint(pt + 8) ^ rk[2];
            var s3 = GeTuint(pt + 12) ^ rk[3];
            // Nr - 1 full rounds:
            var r = nr >> 1;
            for (;;)
            {
                t0 =
                    Te0[(s0 >> 24)] ^
                    Te1[(s1 >> 16) & 0xff] ^
                    Te2[(s2 >> 8) & 0xff] ^
                    Te3[(s3) & 0xff] ^
                    rk[4];
                t1 =
                    Te0[(s1 >> 24)] ^
                    Te1[(s2 >> 16) & 0xff] ^
                    Te2[(s3 >> 8) & 0xff] ^
                    Te3[(s0) & 0xff] ^
                    rk[5];
                t2 =
                    Te0[(s2 >> 24)] ^
                    Te1[(s3 >> 16) & 0xff] ^
                    Te2[(s0 >> 8) & 0xff] ^
                    Te3[(s1) & 0xff] ^
                    rk[6];
                t3 =
                    Te0[(s3 >> 24)] ^
                    Te1[(s0 >> 16) & 0xff] ^
                    Te2[(s1 >> 8) & 0xff] ^
                    Te3[(s2) & 0xff] ^
                    rk[7];

                rk += 8;
                if (--r == 0)
                {
                    break;
                }

                s0 =
                    Te0[(t0 >> 24)] ^
                    Te1[(t1 >> 16) & 0xff] ^
                    Te2[(t2 >> 8) & 0xff] ^
                    Te3[(t3) & 0xff] ^
                    rk[0];
                s1 =
                    Te0[(t1 >> 24)] ^
                    Te1[(t2 >> 16) & 0xff] ^
                    Te2[(t3 >> 8) & 0xff] ^
                    Te3[(t0) & 0xff] ^
                    rk[1];
                s2 =
                    Te0[(t2 >> 24)] ^
                    Te1[(t3 >> 16) & 0xff] ^
                    Te2[(t0 >> 8) & 0xff] ^
                    Te3[(t1) & 0xff] ^
                    rk[2];
                s3 =
                    Te0[(t3 >> 24)] ^
                    Te1[(t0 >> 16) & 0xff] ^
                    Te2[(t1 >> 8) & 0xff] ^
                    Te3[(t2) & 0xff] ^
                    rk[3];
            }
            // apply last round and
            // map cipher state to byte array block:
            s0 =
                (Te4[(t0 >> 24)] & 0xff000000) ^
                (Te4[(t1 >> 16) & 0xff] & 0x00ff0000) ^
                (Te4[(t2 >> 8) & 0xff] & 0x0000ff00) ^
                (Te4[(t3) & 0xff] & 0x000000ff) ^
                rk[0];
            PuTuint(ct, s0);
            s1 =
                (Te4[(t1 >> 24)] & 0xff000000) ^
                (Te4[(t2 >> 16) & 0xff] & 0x00ff0000) ^
                (Te4[(t3 >> 8) & 0xff] & 0x0000ff00) ^
                (Te4[(t0) & 0xff] & 0x000000ff) ^
                rk[1];
            PuTuint(ct + 4, s1);
            s2 =
                (Te4[(t2 >> 24)] & 0xff000000) ^
                (Te4[(t3 >> 16) & 0xff] & 0x00ff0000) ^
                (Te4[(t0 >> 8) & 0xff] & 0x0000ff00) ^
                (Te4[(t1) & 0xff] & 0x000000ff) ^
                rk[2];
            PuTuint(ct + 8, s2);
            s3 =
                (Te4[(t3 >> 24)] & 0xff000000) ^
                (Te4[(t0 >> 16) & 0xff] & 0x00ff0000) ^
                (Te4[(t1 >> 8) & 0xff] & 0x0000ff00) ^
                (Te4[(t2) & 0xff] & 0x000000ff) ^
                rk[3];
            PuTuint(ct + 12, s3);
        }

        public static void RijndaelDecrypt(uint* rk /*4*(Nr + 1)*/, int nr, byte* ct, byte*pt)
        {
            uint t0;
            uint t1;
            uint t2;
            uint t3;

            // map byte array block to cipher state
            // and add initial round key:
            var s0 = GeTuint(ct) ^ rk[0];
            var s1 = GeTuint(ct + 4) ^ rk[1];
            var s2 = GeTuint(ct + 8) ^ rk[2];
            var s3 = GeTuint(ct + 12) ^ rk[3];

            /*
             * Nr - 1 full rounds:
             */
            var r = nr >> 1;
            for (;;)
            {
                t0 =
                    Td0[(s0 >> 24)] ^
                    Td1[(s3 >> 16) & 0xff] ^
                    Td2[(s2 >> 8) & 0xff] ^
                    Td3[(s1) & 0xff] ^
                    rk[4];
                t1 =
                    Td0[(s1 >> 24)] ^
                    Td1[(s0 >> 16) & 0xff] ^
                    Td2[(s3 >> 8) & 0xff] ^
                    Td3[(s2) & 0xff] ^
                    rk[5];
                t2 =
                    Td0[(s2 >> 24)] ^
                    Td1[(s1 >> 16) & 0xff] ^
                    Td2[(s0 >> 8) & 0xff] ^
                    Td3[(s3) & 0xff] ^
                    rk[6];
                t3 =
                    Td0[(s3 >> 24)] ^
                    Td1[(s2 >> 16) & 0xff] ^
                    Td2[(s1 >> 8) & 0xff] ^
                    Td3[(s0) & 0xff] ^
                    rk[7];

                rk += 8;
                if (--r == 0)
                {
                    break;
                }

                s0 =
                    Td0[(t0 >> 24)] ^
                    Td1[(t3 >> 16) & 0xff] ^
                    Td2[(t2 >> 8) & 0xff] ^
                    Td3[(t1) & 0xff] ^
                    rk[0];
                s1 =
                    Td0[(t1 >> 24)] ^
                    Td1[(t0 >> 16) & 0xff] ^
                    Td2[(t3 >> 8) & 0xff] ^
                    Td3[(t2) & 0xff] ^
                    rk[1];
                s2 =
                    Td0[(t2 >> 24)] ^
                    Td1[(t1 >> 16) & 0xff] ^
                    Td2[(t0 >> 8) & 0xff] ^
                    Td3[(t3) & 0xff] ^
                    rk[2];
                s3 =
                    Td0[(t3 >> 24)] ^
                    Td1[(t2 >> 16) & 0xff] ^
                    Td2[(t1 >> 8) & 0xff] ^
                    Td3[(t0) & 0xff] ^
                    rk[3];
            }

            /*
             * apply last round and
             * map cipher state to byte array block:
             */
            s0 =
                (Td4[(t0 >> 24)] & 0xff000000) ^
                (Td4[(t3 >> 16) & 0xff] & 0x00ff0000) ^
                (Td4[(t2 >> 8) & 0xff] & 0x0000ff00) ^
                (Td4[(t1) & 0xff] & 0x000000ff) ^
                rk[0];
            PuTuint(pt, s0);
            s1 =
                (Td4[(t1 >> 24)] & 0xff000000) ^
                (Td4[(t0 >> 16) & 0xff] & 0x00ff0000) ^
                (Td4[(t3 >> 8) & 0xff] & 0x0000ff00) ^
                (Td4[(t2) & 0xff] & 0x000000ff) ^
                rk[1];
            PuTuint(pt + 4, s1);
            s2 =
                (Td4[(t2 >> 24)] & 0xff000000) ^
                (Td4[(t1 >> 16) & 0xff] & 0x00ff0000) ^
                (Td4[(t0 >> 8) & 0xff] & 0x0000ff00) ^
                (Td4[(t3) & 0xff] & 0x000000ff) ^
                rk[2];
            PuTuint(pt + 8, s2);
            s3 =
                (Td4[(t3 >> 24)] & 0xff000000) ^
                (Td4[(t2 >> 16) & 0xff] & 0x00ff0000) ^
                (Td4[(t1 >> 8) & 0xff] & 0x0000ff00) ^
                (Td4[(t0) & 0xff] & 0x000000ff) ^
                rk[3];
            PuTuint(pt + 12, s3);
        }

        /* setup key context for encryption only */
        public static int rijndael_set_key_enc_only(RijndaelCtx* ctx, byte* key, int bits)
        {
            var rounds = RijndaelKeySetupEnc(ctx->Ek, key, bits);
            if (rounds == 0)
                return -1;

            ctx->Nr = rounds;
            ctx->EncOnly = 1;

            return 0;
        }

        /* setup key context for both encryption and decryption */
        public static int rijndael_set_key(RijndaelCtx*ctx, byte*key, int bits)
        {
            var rounds = RijndaelKeySetupEnc(ctx->Ek, key, bits);
            if (rounds == 0)
                return -1;
            if (RijndaelKeySetupDec(ctx->Dk, key, bits) != rounds)
                return -1;

            ctx->Nr = rounds;
            ctx->EncOnly = 0;

            return 0;
        }

        public static void rijndael_decrypt(RijndaelCtx* ctx, byte* src, byte* dst)
        {
            RijndaelDecrypt(ctx->Dk, ctx->Nr, src, dst);
        }

        public static void rijndael_encrypt(RijndaelCtx*ctx, byte*src, byte*dst)
        {
            RijndaelEncrypt(ctx->Ek, ctx->Nr, src, dst);
        }

        public static int AES_set_key(AesCtx*ctx, byte*key, int bits)
        {
            return rijndael_set_key((RijndaelCtx*) ctx, key, bits);
        }

        public static void AES_decrypt(AesCtx* ctx, byte* src, byte* dst)
        {
            RijndaelDecrypt(ctx->Dk, ctx->Nr, src, dst);
        }

        public static void AES_encrypt(AesCtx* ctx, byte* src, byte* dst)
        {
            RijndaelEncrypt(ctx->Ek, ctx->Nr, src, dst);
        }

        private static void xor_128(byte*a, byte*b, byte*Out)
        {
            for (var i = 0; i < 16; i++)
                Out[i] = (byte) (a[i] ^ b[i]);
        }

        //No IV support!
        public static void AES_cbc_encrypt(AesCtx* ctx, byte* src, byte* dst, int size)
        {
            var blockBuffBytes = new byte[16];
            fixed (byte* blockBuff = blockBuffBytes)
            {
                int i;
                for (i = 0; i < size; i += 16)
                {
                    //step 1: copy block to dst
                    Memcpy(dst, src, 16);
                    //step 2: XOR with previous block
                    if (i != 0) xor_128(dst, blockBuff, dst);
                    //step 3: encrypt the block -> it land in block buffer
                    AES_encrypt(ctx, dst, blockBuff);
                    //step 4: copy back the encrypted block to destination
                    Memcpy(dst, blockBuff, 16);

                    dst += 16;
                    src += 16;
                }
            }
        }

        public static void AES_cbc_decrypt(AesCtx* ctx, byte* src, byte* dst, int size)
        {
            var blockBuffBytes = new byte[16];
            var blockBuffPreviousBytes = new byte[16];
            fixed (byte* blockBuff = blockBuffBytes)
            fixed (byte* blockBuffPrevious = blockBuffPreviousBytes)
            {
                Memcpy(blockBuff, src, 16);
                Memcpy(blockBuffPrevious, src, 16);
                AES_decrypt(ctx, src, dst);

                dst += 16;
                src += 16;

                int i;
                for (i = 16; i < size; i += 16)
                {
                    //step1: backup current block for next block decrypt
                    Memcpy(blockBuff, src, 16);
                    //step2: copy current block to destination
                    Memcpy(dst, src, 16);
                    //step3: decrypt current buffer in place
                    AES_decrypt(ctx, dst, dst);
                    //step4: XOR current buffer with previous buffer
                    xor_128(dst, blockBuffPrevious, dst);
                    //step5: swap buffers
                    Memcpy(blockBuffPrevious, blockBuff, 16);

                    dst += 16;
                    src += 16;
                }
            }
        }

        /* AES-CMAC Generation Function */

        public static void leftshift_onebit(byte* input, byte* output)
        {
            int i;
            byte overflow = 0;

            for (i = 15; i >= 0; i--)
            {
                output[i] = (byte) (input[i] << 1);
                output[i] |= overflow;
                overflow = (byte) (((input[i] & 0x80) != 0) ? 1 : 0);
            }
        }

        public static void generate_subkey(AesCtx* ctx, byte* k1, byte* k2)
        {
            var lBytes = new byte[16];
            var zBytes = new byte[16];
            var tmpBytes = new byte[16];

            fixed (byte* tmp = tmpBytes)
            fixed (byte* l = lBytes)
            fixed (byte* z = zBytes)
            fixed (byte* constRb = ConstRb)
            {
                int i;
                for (i = 0; i < 16; i++) z[i] = 0;

                AES_encrypt(ctx, z, l);

                if ((l[0] & 0x80) == 0) /* If MSB(L) = 0, then K1 = L << 1 */
                {
                    leftshift_onebit(l, k1);
                }
                else
                {
                    /* Else K1 = ( L << 1 ) (+) Rb */
                    leftshift_onebit(l, tmp);
                    xor_128(tmp, constRb, k1);
                }

                if ((k1[0] & 0x80) == 0)
                {
                    leftshift_onebit(k1, k2);
                }
                else
                {
                    leftshift_onebit(k1, tmp);
                    xor_128(tmp, constRb, k2);
                }
            }
        }

        public static void Padding(byte* lastb, byte* pad, int length)
        {
            for (var j = 0; j < 16; j++)
            {
                if (j < length)
                {
                    pad[j] = lastb[j];
                }
                else if (j == length)
                {
                    pad[j] = 0x80;
                }
                else
                {
                    pad[j] = 0x00;
                }
            }
        }

        public static void AES_CMAC(AesCtx* ctx, byte* input, int length, byte* mac)
        {
            var xBytes = new byte[16];
            var yBytes = new byte[16];
            var mLastBytes = new byte[16];
            var paddedBytes = new byte[16];
            var k1Bytes = new byte[16];
            var k2Bytes = new byte[16];

            fixed (byte* x = xBytes)
            fixed (byte* y = yBytes)
            fixed (byte* mLast = mLastBytes)
            fixed (byte* padded = paddedBytes)
            fixed (byte* k1 = k1Bytes)
            fixed (byte* k2 = k2Bytes)
            {
                generate_subkey(ctx, k1, k2);

                var n = (length + 15) / 16;

                int flag;
                if (n == 0)
                {
                    n = 1;
                    flag = 0;
                }
                else
                {
                    flag = (length % 16) == 0 ? 1 : 0;
                }

                if (flag != 0)
                {
                    /* last block is complete block */
                    xor_128(&input[16 * (n - 1)], k1, mLast);
                }
                else
                {
                    Padding(&input[16 * (n - 1)], padded, length % 16);
                    xor_128(padded, k2, mLast);
                }

                int i;
                for (i = 0; i < 16; i++) x[i] = 0;
                for (i = 0; i < n - 1; i++)
                {
                    xor_128(x, &input[16 * i], y); /* Y := Mi (+) X  */
                    AES_encrypt(ctx, y, x); /* X := AES-128(KEY, Y); */
                }

                xor_128(x, mLast, y);
                AES_encrypt(ctx, y, x);

                for (i = 0; i < 16; i++)
                    mac[i] = x[i];
            }
        }

        public static void AES_CMAC_forge(AesCtx* ctx, byte* input, int length, byte* forge)
        {
            var xBytes = new byte[16];
            var yBytes = new byte[16];
            var mLastBytes = new byte[16];
            var paddedBytes = new byte[16];
            var k1Bytes = new byte[16];
            var k2Bytes = new byte[16];

            fixed (byte* x = xBytes)
            fixed (byte* y = yBytes)
            fixed (byte* mLast = mLastBytes)
            fixed (byte* padded = paddedBytes)
            fixed (byte* k1 = k1Bytes)
            fixed (byte* k2 = k2Bytes)
            {
                generate_subkey(ctx, k1, k2);

                var n = (length + 15) / 16;

                int flag;
                if (n == 0)
                {
                    n = 1;
                    flag = 0;
                }
                else
                {
                    flag = (length % 16) == 0 ? 1 : 0;
                }

                if (flag != 0)
                {
                    /* last block is complete block */
                    xor_128(&input[16 * (n - 1)], k1, mLast);
                }
                else
                {
                    Padding(&input[16 * (n - 1)], padded, length % 16);
                    xor_128(padded, k2, mLast);
                }

                int i;
                for (i = 0; i < 16; i++) x[i] = 0;
                for (i = 0; i < n - 1; i++)
                {
                    xor_128(x, &input[16 * i], y); /* Y := Mi (+) X  */
                    AES_encrypt(ctx, y, x); /* X := AES-128(KEY, Y); */
                }

                xor_128(x, mLast, y);
                AES_decrypt(ctx, forge, x);
                //printf("Pre-crypt value: "); for(i=0;i<0x10;i++) printf("%02x", X[i]); printf("\n");
                xor_128(x, y, forge);
                xor_128(forge, &input[16 * (n - 1)], y);
                //AES_encrypt(Y, X, &aes);

                //Update original input file so it produces the correct CMAC
                for (i = 0; i < 16; i++)
                {
                    input[(16 * (n - 1)) + i] = y[i];
                }
            }
        }
    }
}