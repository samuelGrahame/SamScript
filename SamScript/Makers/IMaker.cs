using SamScript.Compile;
using System;
using System.Collections.Generic;
using System.Text;

namespace SamScript.Makers
{
    public interface IMaker
    {
        void Make(Compiler compiler);
        void MakeFile(SamFile file);
        void MakeField(SamFileField field, StringBuilder builder);
        void MakeMethod(SamFileMethod method, StringBuilder builder);
        void MakeMethodBody(SamFileMethod method, StringBuilder builder);
    }
}
