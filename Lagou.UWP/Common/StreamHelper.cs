﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagou.UWP.Common {
    public static class StreamHelper {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stm"></param>
        /// <param name="perCount"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytes(this Stream stm, int perCount = 1024) {
            if (stm == null)
                throw new ArgumentNullException("stm");
            if (perCount <= 0)
                throw new ArgumentOutOfRangeException("perCount", "perCount 必须大于等于0");

            if (stm.CanSeek)
                stm.Position = 0;

            byte[] bytes = new byte[stm.Length];
            var offset = 0;
            var count = 0;
            while (0 != (count = await stm.ReadAsync(bytes, offset, stm.Length - stm.Position > perCount ? perCount : (int)(stm.Length - stm.Position)))) {
                offset += count;
            }

            return bytes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stm"></param>
        /// <returns></returns>
        public static async Task<string> ToBase64Url(this Stream stm) {
            return Convert.ToBase64String(await stm.GetBytes());
        }

    }
}
