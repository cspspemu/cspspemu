#region (c)2008-2013 Hawkynt
/*
 *  cImage 
 *  Image filtering library 
    Copyright (C) 2010-2013 Hawkynt

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

#define PREFERARRAYCACHE
using System;
using dword = System.UInt32;
namespace Imager {
  /// <summary>
  /// A little cache that holds calculation results based on the three color components red, green and blue.
  /// </summary>
  public class cRGBCache {
#if PREFERARRAYCACHE
    private readonly byte[] _valueCache = new byte[256 * 256 * 256];
    private readonly byte[] _existsCache = new byte[256 * 256 * 256];

    /// <summary>
    /// Gets a value directly from the cache or first calculates that value and writes it the cache.
    /// </summary>
    /// <param name="key">The 32-bit color code.</param>
    /// <param name="factory">The factory that would calculate a result if it's not already in the cache.</param>
    /// <returns>The calculation result.</returns>
    public byte GetOrAdd(dword key, Func<dword, byte> factory) {
      if (this._existsCache[key] != 0)
        return (this._valueCache[key]);

      var result = factory(key);
      this._valueCache[key] = result;
      this._existsCache[key] = 1;
      System.Threading.Thread.MemoryBarrier();
      return (result);
    }
#else
    /// <summary>
    /// Our thread-safe dictionary cache
    /// </summary>
    private readonly ConcurrentDictionary<UInt32, byte> _cache = new ConcurrentDictionary<uint, byte>();
    /// <summary>
    /// Gets a value directly from the cache or first calculates that value and writes it the cache.
    /// </summary>
    /// <param name="key">The 32-bit color code.</param>
    /// <param name="factory">The factory that would calculate a result if it's not already in the cache.</param>
    /// <returns>The calculation result.</returns>
    public byte GetOrAdd(dword key, Func<dword, byte> factory) {
      return (this._cache.GetOrAdd(key, factory));
    }
#endif
  } // end class
} // end namespace