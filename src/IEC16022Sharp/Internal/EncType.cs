/** 
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
    internal enum EncType
    {
        E_ASCII = 0,
        E_C40,
        E_TEXT,
        E_X12,
        E_EDIFACT,
        E_BINARY,
        E_MAX
    };
}
