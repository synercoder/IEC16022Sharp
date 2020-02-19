/*
 *
 * IEC16022Sharp DataMatrix bar code generation lib
 *   Fabrizio Accatino
 * 
 *   C# porting of:
 *      IEC16022 bar code generation
 *      Adrian Kennard, Andrews & Arnold Ltd
 *      with help from Cliff Hones on the RS coding
 *      (C version currently maintained by Stefan Schmidt)
 * 
 * (c) 2004 Adrian Kennard, Andrews & Arnold Ltd
 * (c) 2006 Stefan Schmidt <stefan@datenfreihafen.org>
 * (c) 2007 Fabrizio Accatino <fhtino@yahoo.com>
 * (c) 2020 Gerard Gunnewijk <gerard.gunnewijk@live.nl>
 * 
 * ----------------------------------------
 * This is a simple Reed-Solomon encoder
 * (C) Cliff Hones 2004
 * ----------------------------------------
 * 
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA
 *
 */

namespace IEC16022Sharp
{
    public class ReedSol
    {
        private int _logmod;	    // 2**symsize - 1
        private int _rlen;

        private int[] _log = null;
        private int[] _alog = null;
        private int[] _rspoly = null;

        public void RsInitGf(int poly)
        {
            int m, b, p, v;

            // C# does not need that:
            //
            // Return storage from previous setup
            //if (log)
            //{
            //    free(log);
            //    free(alog);
            //    free(rspoly);
            //    rspoly = NULL;
            //}

            // Find the top bit, and hence the symbol size
            for (b = 1, m = 0; b <= poly; b <<= 1)
                m++;
            b >>= 1;
            m--;

            // Calculate the log/alog tables
            _logmod = ( 1 << m ) - 1;
            _log = new int[_logmod + 1];  // C:  log = (int*)malloc(sizeof(int) * (logmod + 1));
            _alog = new int[_logmod];     // C:  alog = (int*)malloc(sizeof(int) * logmod);

            for (p = 1, v = 0; v < _logmod; v++)
            {
                _alog[v] = p;
                _log[p] = v;
                p <<= 1;
                if (( p & b ) != 0)
                    p ^= poly;
            }
        }

        public void RsInitCode(int nsym, int index)
        {
            int i, k;

            // C# does not need that:
            //if (rspoly)
            //    free(rspoly);

            _rspoly = new int[nsym + 1];   // C:  rspoly = (int*)malloc(sizeof(int) * (nsym + 1));

            _rlen = nsym;

            _rspoly[0] = 1;
            for (i = 1; i <= nsym; i++)
            {
                _rspoly[i] = 1;
                for (k = i - 1; k > 0; k--)
                {
                    if (_rspoly[k] != 0)
                        _rspoly[k] =
                            _alog[( _log[_rspoly[k]] + index ) % _logmod];
                    _rspoly[k] ^= _rspoly[k - 1];
                }
                _rspoly[0] = _alog[( _log[_rspoly[0]] + index ) % _logmod];
                index++;
            }
        }

        public void RsEncode(int len, byte[] data, out byte[] res)
        {
            int i, k, m;

            res = new byte[_rlen];
            for (i = 0; i < _rlen; i++)
                res[i] = 0;
            for (i = 0; i < len; i++)
            {
                m = res[_rlen - 1] ^ data[i];
                for (k = _rlen - 1; k > 0; k--)
                {
                    if (m != 0 && _rspoly[k] != 0)
                        res[k] = (byte)( res[k - 1] ^ _alog[( _log[m] + _log[_rspoly[k]] ) % _logmod] );
                    else
                        res[k] = res[k - 1];
                }
                if (m != 0 && _rspoly[0] != 0)
                    res[0] = (byte)_alog[( _log[m] + _log[_rspoly[0]] ) % _logmod];
                else
                    res[0] = 0;
            }
        }
    }
}
