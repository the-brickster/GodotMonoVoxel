using Godot;
using System;
using System.Linq;

namespace Goxlap.src.Goxlap.utils
{
    class TestMorton3D
    {
        protected Morton3D mortonTest;
        private static int[] control_3D_Encode = {
            0, 4, 32, 36, 256, 260, 288, 292, 2048, 2052, 2080, 2084, 2304, 2308, 2336, 2340, 2,
                6, 34, 38, 258, 262, 290, 294, 2050, 2054, 2082, 2086, 2306, 2310, 2338, 1753,
                1757, 1785, 1789, 2009, 2013, 2041, 2045, 3801, 3805, 3833, 3837, 4057, 4061, 4089, 4093, 1755,
                1759, 1787, 1791, 2011, 2015, 2043, 2047, 3803, 3807, 3835, 3839, 4059, 4063, 4091, 4095
        };

        private static int[][] control_3D_Decode = {
            new int[]{0, 0, 0}, new int[]{0, 0, 1}, new int[]{0, 0, 2}, new int[]{0, 0, 3}, new int[]{0, 0, 4}, new int[]{0, 0, 5}, new int[]{0, 0, 6},
            new int[]{0, 0, 7}, new int[]{0, 0, 8}, new int[]{0, 0, 9}, new int[]{0, 0, 10}, new int[]{0, 0, 11},new int[] {0, 0, 12}, new int[]{0, 0, 13},
            new int[]{0, 0, 14}, new int[]{0, 0, 15},new int[] {0, 1, 0}, new int[]{0, 1, 1}, new int[]{0, 1, 2},new int[] {0, 1, 3}, new int[]{0, 1, 4},
            new int[]{0, 1, 5}, new int[]{0, 1, 6}, new int[]{0, 1, 7}, new int[]{0, 1, 8}, new int[]{0, 1, 9}, new int[]{0, 1, 10}, new int[]{0, 1, 11},
            new int[] {0, 1, 12}, new int[]{0, 1, 13},new int[] {0, 1, 14}, new int[]{15, 14, 0}, new int[]{15, 14, 1}, new int[]{15, 14, 2},
            new int[]{15, 14, 3},new int[] {15, 14, 4}, new int[]{15, 14, 5}, new int[]{15, 14, 6}, new int[]{15, 14, 7}, new int[]{15, 14, 8},
            new int[] {15, 14, 9},new int[] {15, 14, 10},new int[] {15, 14, 11},new int[] {15, 14, 12},new int[] {15, 14, 13},new int[] {15, 14, 14},
            new int[] {15, 14, 15}, new int[]{15, 15, 0},new int[] {15, 15, 1},new int[] {15, 15, 2}, new int[]{15, 15, 3}, new int[]{15, 15, 4},
            new int[]{15, 15, 5}, new int[]{15, 15, 6}, new int[]{15, 15, 7}, new int[]{15, 15, 8}, new int[]{15, 15, 9}, new int[]{15, 15, 10},
            new int[] {15, 15, 11},new int[]{15, 15, 12}, new int[]{15, 15, 13}, new int[]{15, 15, 14}, new int[]{15, 15, 15}
        };

        public void setup()
        {
            mortonTest = new Morton3D();
        }

        public void testEncode()
        {
            try
            {
                Random random = new Random();
                for (int i = 0; i < 1024; i++)
                {
                    
                    int x = (int)(random.Next() * 2097151);
                    int y = (int)(random.Next() * 2097151);
                    int z = (int)(random.Next() * 2097151);

                    mortonTest.encode(x, y, z);
                }
                Console.WriteLine("Test Encode method failed, it did not throw an error as expected");
            }
            catch (System.Exception)
            {

                throw;
            }
            for (int i = 0; i < 63; i++) {
                if (mortonTest.encode(control_3D_Decode[i][0], control_3D_Decode[i][1], control_3D_Decode[i][2]) != control_3D_Encode[i]){
                    Console.WriteLine("Does not equals {0}, {1}",mortonTest.encode(control_3D_Decode[i][0], control_3D_Decode[i][1], control_3D_Decode[i][2]),control_3D_Encode[i]);
                }
            }
        }

            public void testDecode() {
        try {
            Random random = new Random();
            for (int i = 0; i < 1024; i++) {
                int c = (int) (random.Next() * Math.Pow(2, 64));
                mortonTest.decode(c);
            }
            Console.WriteLine("My method didn't throw when I expected it to");
        } catch (System.Exception ex) {
        }
        for (int i = 0; i < 63; i++) {
            if(!Enumerable.Equals(mortonTest.decode(control_3D_Encode[i]), new int[]{control_3D_Decode[i][0], control_3D_Decode[i][1], control_3D_Decode[i][2]})){
                Console.WriteLine("Does not equal [{0}], |{1},{2},{3}|", string.Join(", ", mortonTest.decode(control_3D_Encode[i])),control_3D_Decode[i][0],
                control_3D_Decode[i][1],control_3D_Decode[i][2]);
            }
        }
    }
    }


}