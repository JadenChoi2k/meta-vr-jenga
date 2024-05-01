/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System.Collections;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace Meta.XR.MRUtilityKit.Tests
{
    public class TriangulatorTests : MonoBehaviour
    {
        private const int DefaultTimeoutMs = 10000;

        private float CalculateTriangleArea(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)) / 2.0f;
        }

        // Use the triangulated area as a proxy to ensure the triangulation worked as expected
        private float CalculateTriangulatedArea(List<Vector2> vertices, List<int> indices)
        {
            float area = 0f;
            for (int i = 0; i < indices.Count; i += 3)
            {
                var p1 = vertices[indices[i]];
                var p2 = vertices[indices[i + 1]];
                var p3 = vertices[indices[i + 2]];
                var triangleArea = CalculateTriangleArea(p1, p2, p3);
                Assert.GreaterOrEqual(triangleArea, 0f);
                area += triangleArea;
            }

            return area;
        }

        /// <summary>
        /// Tests that the triangulator is able to triangulate a simple quad
        /// </summary>
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TriangulateQuad()
        {
            var vertices = new List<Vector2> { new(0f, 0f), new(1f, 0f), new(1f, 1f), new(0f, 1f) };
            var indices = Triangulator.TriangulatePoints(vertices);
            yield return null;
            Assert.AreEqual(6, indices.Count);
            Assert.AreEqual(1.0f, CalculateTriangulatedArea(vertices, indices));
        }

        /// <summary>
        /// Tests that the triangulator is able to triangulate a 2x2 quad with a 1x1 quad hole in the center of it
        /// </summary>
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TriangulateQuadWithHole()
        {
            var vertices = new List<Vector2> { new(0f, 0f), new(2f, 0f), new(2f, 2f), new(0f, 2f) };
            var holes = new List<List<Vector2>> { new List<Vector2> { new(0.5f, 0.5f), new(0.5f, 1.5f), new(1.5f, 1.5f), new(1.5f, 0.5f) } };
            var outline = Triangulator.CreateOutline(vertices, holes);
            var indices = Triangulator.TriangulateMesh(outline);
            yield return null;
            Assert.AreEqual(24, indices.Count);
            Assert.AreEqual(3.0f, CalculateTriangulatedArea(outline.vertices, indices));
        }

        /// <summary>
        /// Tests that the triangulator is able to triangulate a 4x4 quad with four 1x1 quad holes distributed in a grid pattern
        /// </summary>
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TriangulateQuadWith4Holes()
        {
            var vertices = new List<Vector2> { new(0f, 0f), new(4f, 0f), new(4f, 4f), new(0f, 4f) };
            var holes = new List<List<Vector2>>();
            for (int i = 0; i < 4; i++)
            {
                var offset = new Vector2(0.5f + 2f * (i / 2), 0.5f + 2f * (i % 2));
                holes.Add(new List<Vector2> { offset + new Vector2(0f, 0f), offset + new Vector2(0f, 1f), offset + new Vector2(1f, 1f), offset + new Vector2(1f, 0f) });
            }
            var outline = Triangulator.CreateOutline(vertices, holes);
            var indices = Triangulator.TriangulateMesh(outline);
            yield return null;
            Assert.AreEqual(78, indices.Count);
            Assert.AreEqual(12.0f, CalculateTriangulatedArea(outline.vertices, indices));
        }

        /// <summary>
        /// Tests that the triangulator is able to triangulate an L shape
        /// </summary>
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TriangulateLShape()
        {
            var vertices = new List<Vector2> { new(0f, 0f), new(2f, 0f), new(2f, 2f), new(1f, 2f), new(1f, 1f), new(0f, 1f) };
            var indices = Triangulator.TriangulatePoints(vertices);
            yield return null;
            Assert.AreEqual(12, indices.Count);
            Assert.AreEqual(3.0f, CalculateTriangulatedArea(vertices, indices));
        }

        /// <summary>
        /// Tests that the triangulator is able to triangulate a C shape
        /// </summary>
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TriangulateCShape()
        {
            var vertices = new List<Vector2> { new(0f, 0f), new(2f, 0f), new(2f, 1f), new(1f, 1f), new(1f, 2f), new(2f, 2f), new(2f, 3f), new(0f, 3f) };
            var indices = Triangulator.TriangulatePoints(vertices);
            yield return null;
            Assert.AreEqual(18, indices.Count);
            Assert.AreEqual(5.0f, CalculateTriangulatedArea(vertices, indices));
        }
    }
}
