using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GLMultidrawIndirectExample
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(uint) * 5)]
    public struct DrawElementsIndirectData
    {
        uint NumVertcies;
        uint NumInstances;
        uint FirstIndex;
        uint BaseVertex;
        uint BaseInstance;

        public DrawElementsIndirectData(
            uint numElemements,
            uint instanceCount,
            uint firstIndex,
            uint baseVertex,
            uint baseInstance)
        {
            NumVertcies = numElemements;
            NumInstances = instanceCount;
            FirstIndex = firstIndex;
            BaseVertex = baseVertex;
            BaseInstance = baseInstance;
        }
    }
}
