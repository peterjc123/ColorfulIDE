// Guids.cs
// MUST match guids.h
using System;

namespace HiTeam.ColorfulIDE
{
    static class GuidList
    {
        public const string GuidColorfulIdePkgString = "6f012ab1-a7eb-48ec-a09a-344442bf17e2";
        public const string GuidColorfulIdeCmdSetString = "a436d208-80da-4c94-b22e-90f6d344035b";

        public static readonly Guid GuidColorfulIdeCmdSet = new Guid(GuidColorfulIdeCmdSetString);
    };
}