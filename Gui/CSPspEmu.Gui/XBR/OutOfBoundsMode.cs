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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Imager.Interface {
  /// <summary>
  /// This tells us how to handle read requests to pixels which are out of image bounds.
  /// </summary>
  public enum OutOfBoundsMode {
    /// <summary>
    /// aaa abcde eee
    /// </summary>
    ConstantExtension = 0,
    /// <summary>
    /// cba abcde edc
    /// </summary>
    HalfSampleSymmetric,
    /// <summary>
    /// dcb abcde dcb
    /// </summary>
    WholeSampleSymmetric,
    /// <summary>
    /// cde abcde abc
    /// </summary>
    WrapAround
  }

  internal static class OutOfBoundsUtils {

    public delegate int OutOfBoundsHandler(int index, int count, bool overflow, bool underflow);

    private static readonly Dictionary<OutOfBoundsMode, OutOfBoundsHandler> _OUT_OF_BOUNDS_HANDLERS = new Dictionary<OutOfBoundsMode, OutOfBoundsHandler> {
      {OutOfBoundsMode.ConstantExtension,_ConstantExtension},
      {OutOfBoundsMode.WrapAround,_WrapAround},
      {OutOfBoundsMode.HalfSampleSymmetric,_HalfSampleSymmetric},
      {OutOfBoundsMode.WholeSampleSymmetric,_WholeSampleSymmetric}
    };

    #region out of bounds handlers
    private static int _ConstantExtension(int index, int count, bool overflow, bool underflow) {
      return (overflow ? count - 1 : 0);
    }

    private static int _WrapAround(int index, int count, bool overflow, bool underflow) {
      /*
        c:2
          -3 -2 -1  0 +1 +2 +3
           1  0  1  0  1  0  1

        c:3
          -3 -2 -1  0 +1 +2 +3
           0  1  2  0  1  2  0
          */

      if (overflow)
        return (index % count);
      /*
      // Loop-version
      if (overflow)
        while(true)
          if(index>=count)
            index-=count;
          else
            return(index);
      */

      return (count - ((-index) % count)) % count;

      /*
      // Loop-Version
      while (index < 0)
        index += count;
      return (index);
      */
    }

    private static int _HalfSampleSymmetric(int index, int count, bool overflow, bool underflow) {

      // FIXME: calculate this without a loop
      while (true) {
        if (index < 0)
          index = -1 - index;
        else if (index >= count)
          index = (2 * count - 1) - index;
        else
          return (index);
      }
    }

    private static int _WholeSampleSymmetric(int index, int count, bool overflow, bool underflow) {

      // FIXME: calculate this without a loop
      while (true) {
        if (index < 0)
          index = -index;
        else if (index >= count)
          index = (2 * count - 2) - index;
        else
          return (index);
      }
    }
    #endregion

    /// <summary>
    /// Gets the out of bounds handler or crashes.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns></returns>
    public static OutOfBoundsHandler GetHandlerOrCrash(OutOfBoundsMode mode) {
      OutOfBoundsHandler result;
      if (_OUT_OF_BOUNDS_HANDLERS.TryGetValue(mode, out result))
        return (result);

      throw new NotSupportedException("The OutOfBoundsMode " + mode + " is not supported");
    }

    /// <summary>
    /// Checks coordinates for over-/underflow and does the correction based on the given OutOfBoundsMode.
    /// </summary>
    /// <param name="index">The coordinate index.</param>
    /// <param name="count">The sample count.</param>
    /// <param name="mode">The mode.</param>
    /// <returns>A coordinate index that is surely between the bounds.</returns>
    public static int GetBoundsCheckedCoordinate(int index, int count, OutOfBoundsMode mode) {
#if !NET35
      Contract.Requires(count > 0, "Number of samples must be above 0");
#endif

      // check bounds
      var underflow = index < 0;
      var overflow = index >= count;
      if (!(overflow || underflow))
        return (index);

      // find handler
      var handler = GetHandlerOrCrash(mode);

      // execute handler
      return (handler(index, count, overflow, underflow));
    }

    /// <summary>
    /// Checks coordinates for over-/underflow and does the correction based on the given OutOfBoundsMode.
    /// </summary>
    /// <param name="index">The coordinate index.</param>
    /// <param name="count">The sample count.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>
    /// A coordinate index that is surely between the bounds.
    /// </returns>
    public static int GetBoundsCheckedCoordinate(int index, int count, OutOfBoundsHandler handler) {
#if !NET35
      Contract.Requires(count > 0, "Number of samples must be above 0");
      Contract.Requires(handler != null, "OutOfBounds handler missing");
#endif

      // check bounds
      var underflow = index < 0;
      var overflow = index >= count;
      if (!(overflow || underflow))
        return (index);

      // execute handler
      return (handler(index, count, overflow, underflow));
    }

  }
}
