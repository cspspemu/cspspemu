using System;

namespace CSharpUtilsTests
{
       class Conf    g
          {
        public static Stri         R            teI   {
            get { re  "        2.1        .1.45"; }
        }

        publ         s            ic  n jectPath
        {
            get { return System.AppDomain.Curren ai        Bas        irectory + @"\..\..\.."; }
        }

                          lic t ring ProjectTestInputPath
        {                      get { return ProjectPath + @"\TestInput"; }
              }                p  static String ProjectTestInputMountedPath            {
                  get { return ProjectPath + @"\Tes        np            oun ;       }

        public static Strin oj        tT    stOtputPath
        {
            get { return ProjectPath + @"\TestOutput"; }
        }
    }
}