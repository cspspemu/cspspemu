using cscodec.av;
using System;

namespace cscodec.h264.decoder
{
    public class DebugTool
    {
        public const bool DEBUG_MODE = false;
        public static int logCount = 0;

        public static void dumpDebugFrameData(H264Context h, string msg)
        {
            if (!DEBUG_MODE) return;
            dumpDebugFrameData(h, msg, true);
        }

        public static void dumpDebugFrameData(H264Context h, string msg, bool incrementCounter)
        {
            if (!DEBUG_MODE) return;

            try
            {
                if (incrementCounter)
                    logCount++;

                Console.WriteLine("Dumping Decoder State(" + msg + "): " + logCount + ", Frame: " + h.frame_num);

                // Dump all data inside decoder
                Console.Write("ctx.non_zero_count_cache: ");
                for (int i = 0; i < h.non_zero_count_cache.Length; i++)
                {
                    Console.Write("," + h.non_zero_count_cache[i]);
                } // for
                Console.WriteLine();

                //for(int j=0;j<h.non_zero_count.Length;j++) {
                int j = h.mb_xy;
                if (h.non_zero_count != null)
                    if (j >= 0 && j < h.non_zero_count.Length)
                    {
                        Console.Write("ctx.non_zero_count[" + j + "]: ");
                        for (int i = 0; i < h.non_zero_count[j].Length; i++)
                        {
                            Console.Write("," + h.non_zero_count[j][i]);
                        } // for i
                        Console.WriteLine();
                    } // for j

                // Dump all data inside decoder
                Console.Write("edge_emu_buffer: ");
                for (int i = 0; i < (h.s.width + 64) * 2 * 21 && logCount == 9537; i++)
                {
                    Console.Write("," + h.s.allocated_edge_emu_buffer[h.s.edge_emu_buffer_offset + i]);
                } // for
                Console.WriteLine();

                Console.Write("ctx.mv_cache[0]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.mv_cache[0][i][0] + "," + h.mv_cache[0][i][1]);
                } // for
                Console.WriteLine();

                Console.Write("ctx.mv_cache[1]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.mv_cache[1][i][0] + "," + h.mv_cache[1][i][1]);
                } // for
                Console.WriteLine();

                Console.Write("ctx.mvd_cache[0]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.mvd_cache[0][i][0] + "," + h.mvd_cache[0][i][1]);
                } // for
                Console.WriteLine();

                Console.Write("ctx.mvd_cache[1]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.mvd_cache[1][i][0] + "," + h.mvd_cache[1][i][1]);
                } // for
                Console.WriteLine();

                if (h.mvd_table[0] != null)
                {
                    Console.Write("ctx.mvd_table[0]: ");
                    for (int i = 0; i < 40; i++)
                    {
                        Console.Write("," + h.mvd_table[0][i][0] + "," + h.mvd_table[0][i][1]);
                    } // for
                    Console.WriteLine();

                    Console.Write("ctx.mvd_table[1]: ");
                    for (int i = 0; i < 40; i++)
                    {
                        Console.Write("," + h.mvd_table[1][i][0] + "," + h.mvd_table[1][i][1]);
                    } // for
                    Console.WriteLine();
                } // if

                Console.Write("ctx.ref_cache[0]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.ref_cache[0][i] + "," + h.ref_cache[0][i]);
                } // for
                Console.WriteLine();

                Console.Write("ctx.ref_cache[1]: ");
                for (int i = 0; i < 40; i++)
                {
                    Console.Write("," + h.ref_cache[1][i] + "," + h.ref_cache[1][i]);
                } // for
                Console.WriteLine();

                Console.Write("error_status_table: ");
                if (h.s.error_status_table != null)
                    for (int i = 0; i < /*h.s.error_status_table.Length*/32 && h.s.error_status_table.Length > 32; i++)
                    {
                        Console.Write("," + h.s.error_status_table[i]);
                    } // for
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
            } // try
        }

        public static void printDebugString(string msg)
        {
            if (!DEBUG_MODE) return;

            try
            {
                if (logCount == 0) logCount++;

                Console.Write(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
            } // try
        }

        public static void dumpFrameData(AVFrame frame)
        {
            if (!DEBUG_MODE) return;

            try
            {
                if (logCount == 0) logCount++;

                Console.WriteLine("****** DUMPING FRAME DATA ******");
                int j = 0;
                for (int i = 0; i < /*2000*/100 /*frame.data_base[0].Length-frame.data_offset[0]*/; i++)
                {
                    if (i % 40 == 0)
                    {
                        Console.WriteLine();
                        Console.Write("[" + j + "]: ");
                        j++;
                    } // if
                    Console.Write("" + frame.data_base[0][frame.data_offset[0] + i /*/*+13338-1024+8*512*/] + ",");
                } // for
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
            } // try
        }
    }
}