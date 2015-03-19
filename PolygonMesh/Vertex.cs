﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

using System;

using MatterHackers.VectorMath;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MatterHackers.PolygonMesh
{
    [DebuggerDisplay("ID = {Data.ID} | XYZ = {Position}")]
    public class Vertex
    {
        public MetaData Data 
        { 
            get 
            {
                return MetaData.Get(this);
            } 
        }

#if false
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
#else
        // this is to save memory on each vertex (12 bytes per positon and 12 per normal rather than 24 and 24)
        Vector3Float position;
        public Vector3 Position 
        {
            get
            {
                return new Vector3(position.x, position.y, position.z); 
            }
            set
            {
                position.x = (float)value.x;
                position.y = (float)value.y;
                position.z = (float)value.z;
            }
        }

        Vector3Float normal;
        public Vector3 Normal
        {
            get
            {
                return new Vector3(normal.x, normal.y, normal.z);
            }
            set
            {
                normal.x = (float)value.x;
                normal.y = (float)value.y;
                normal.z = (float)value.z;
            }
        }
#endif

        public MeshEdge firstMeshEdge;

        public Vertex(Vector3 position)
        {
            this.Position = position;
        }

        public virtual Vertex CreateInterpolated(Vertex dest, double ratioToDest)
        {
            Vertex interpolatedVertex = new Vertex(Vector3.Lerp(this.Position, dest.Position, ratioToDest));
            interpolatedVertex.Normal = Vector3.Lerp(this.Normal, dest.Normal, ratioToDest).GetNormal();
            return interpolatedVertex;
        }

        public void AddDebugInfo(StringBuilder totalDebug, int numTabs)
        {
            int firstMeshEdgeID = -1;
            if (firstMeshEdge != null)
            {
                firstMeshEdgeID = firstMeshEdge.Data.ID;
            }
            totalDebug.Append(new string('\t', numTabs) + String.Format("First MeshEdge: {0}\n", firstMeshEdgeID));
            if (firstMeshEdge != null)
            {
                firstMeshEdge.AddDebugInfo(totalDebug, numTabs + 1);
            }
        }

        public IEnumerable<Face> ConnectedFaces()
        {
            HashSet<Face> allFacesOfThisEdge = new HashSet<Face>();
            foreach (MeshEdge meshEdge in ConnectedMeshEdges())
            {
                foreach (Face face in meshEdge.FacesSharingMeshEdgeIterator())
                {
                    allFacesOfThisEdge.Add(face);
                }
            }

            foreach (Face face in allFacesOfThisEdge)
            {
                yield return face;
            }
        }

        public List<MeshEdge> GetConnectedMeshEdges()
        {
            List<MeshEdge> meshEdgeList = new List<MeshEdge>();
            foreach (MeshEdge meshEdge in ConnectedMeshEdges())
            {
                meshEdgeList.Add(meshEdge);
            }

            return meshEdgeList;
        }

        public IEnumerable<MeshEdge> ConnectedMeshEdges()
        {
            if (this.firstMeshEdge != null)
            {
                MeshEdge curMeshEdge = this.firstMeshEdge;
                do
                {
                    yield return curMeshEdge;

                    curMeshEdge = curMeshEdge.GetNextMeshEdgeConnectedTo(this);
                } while (curMeshEdge != this.firstMeshEdge);
            }
        }

        public MeshEdge GetMeshEdgeConnectedToVertex(Vertex vertexToFindConnectionTo)
        {
            if (this.firstMeshEdge == null)
            {
                return null;
            }

            foreach (MeshEdge meshEdge in ConnectedMeshEdges())
            {
                if (meshEdge.IsConnectedTo(vertexToFindConnectionTo))
                {
                    return meshEdge;
                }
            }

            return null;
        }

        public int GetConnectedMeshEdgesCount()
        {
            int numConnectedMeshEdges = 0;
            foreach (MeshEdge edge in ConnectedMeshEdges())
            {
                numConnectedMeshEdges++;
            }

            return numConnectedMeshEdges;
        }

        public void Validate()
        {
            if (firstMeshEdge != null)
            {
                HashSet<MeshEdge> foundEdges = new HashSet<MeshEdge>();

                foreach (MeshEdge meshEdge in this.ConnectedMeshEdges())
                {
                    if (foundEdges.Contains(meshEdge))
                    {
                        // TODO: this should realy not be happening. We should only ever try to iterate to any mesh edge once.
                        // We can get an infinite recursion with this and it needs to be debuged.
                        throw new Exception("Bad ConnectedMeshEdges");
                    }

                    foundEdges.Add(meshEdge);
                }
            }
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}
