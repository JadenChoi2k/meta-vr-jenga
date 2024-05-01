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
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Meta.XR.MRUtilityKit.Tests
{
    public class CRUDTests : MonoBehaviour
    {
        private const int DefaultTimeoutMs = 10000 * 1000;
        private MRUKRoom _currentRoom;

        private static string SceneWithRoom2 =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""AA621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""AA6F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""AAA8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""AAC4D6D7094448EEA20403AE86EE6EC1"",""AA800E8EB9EE4C138FDE577C993EA90B"",""AA282AA1CE7B446388350D3D92A48848"",""AA8EAAA0CEC14186B1FF44A9DF52A2B1"",""AA22A765695745529594E95CA5B92E8C"",""AA96ABFBDD3744EDB7BF1417081005FD"",""AA9EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""AA6F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""AAA8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""AAC4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""AA800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""AA282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""AA8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""AA22A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""AA96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""AA9EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""AA53E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""AA67E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""AAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithScene1NewRoomGUID =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""BB621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1SceneAnchorLabelChanged =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""BED""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1SceneAnchorPlaneRectChanged =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4,-1.5],""Max"":[0.4,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1SceneAnchorVolumeBoundsChanged =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.3,-0.8,-1.1],""Max"":[0.3,0.8,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1SceneAnchorPlaneBoundaryChanged =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.7,-1.5],[1.7,-1.5],[1.7,1.5],[-1.7,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1Room3 =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]},{""UUID"":""36621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""C36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""34A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""01C4D6D7094448EEA20403AE86EE6EC1"",""43800E8EB9EE4C138FDE577C993EA90B"",""B6282AA1CE7B446388350D3D92A48848"",""FE8EAAA0CEC14186B1FF44A9DF52A2B1"",""A822A765695745529594E95CA5B92E8C"",""5D96ABFBDD3744EDB7BF1417081005FD"",""899EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""C36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""34A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""01C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""43800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""B6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""FE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""A822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""5D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""899EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""7853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""E767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""DAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom3Room1 =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""36621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""C36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""34A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""01C4D6D7094448EEA20403AE86EE6EC1"",""43800E8EB9EE4C138FDE577C993EA90B"",""B6282AA1CE7B446388350D3D92A48848"",""FE8EAAA0CEC14186B1FF44A9DF52A2B1"",""A822A765695745529594E95CA5B92E8C"",""5D96ABFBDD3744EDB7BF1417081005FD"",""899EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""C36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""34A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""01C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""43800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""B6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""FE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""A822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""5D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""899EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""7853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""E767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""DAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]},{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1MoreAnchors =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}},{""UUID"":""D767E0E46C1742A88813BFA49B8B28F0"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[-1.22,1.27151489,-1.72],""Rotation"":[270,96.27296,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.199005172,-0.401000977,-1.26599121],""Max"":[0.199005172,0.401000977,0]}},{""UUID"":""CAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]},{""UUID"":""DAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]},{""UUID"":""EAE852DB8847498F8A5812EA04F6C1F6"",""SemanticClassifications"":[""TABLE""],""Transform"":{""Translation"":[1.555448,1.06253052,-0.8596737],""Rotation"":[270,1.2390064,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.400512785,-0.9039919],""Max"":[0.400512785,0.9039919]},""VolumeBounds"":{""Min"":[-0.400512785,-0.9039919,-1.05700684],""Max"":[0.400512785,0.9039919,0]},""PlaneBoundary2D"":[[-0.400512785,-0.9039919],[0.400512785,-0.9039919],[0.400512785,0.9039919],[-0.400512785,0.9039919]]}]}]}";
        private static string SceneWithRoom1LessAnchors =
            @"{""CoordinateSystem"":""Unity"",""Rooms"":[{""UUID"":""26621F6BC6E04FC09FF7F37167AD92B7"",""RoomLayout"":{""FloorUuid"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""CeilingUuid"":""24A8F474833744DFB02C488A9BA111E8"",""WallsUuid"":[""91C4D6D7094448EEA20403AE86EE6EC1"",""33800E8EB9EE4C138FDE577C993EA90B"",""A6282AA1CE7B446388350D3D92A48848"",""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""F822A765695745529594E95CA5B92E8C"",""4D96ABFBDD3744EDB7BF1417081005FD"",""799EB9126BD94EFDA93418DDBEB28B99""]},""Anchors"":[{""UUID"":""B36F3D4FDD3C4E05A9591402F93F61E5"",""SemanticClassifications"":[""FLOOR""],""Transform"":{""Translation"":[-1.06952024,0,1.2340889],""Rotation"":[270,273.148438,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-0.614610434,3.14264679],[-3.1610744,3.185142],[-3.16107464,-0.628885269],[0.0159805864,-0.3973337],[-0.00542993844,-3.17527843],[3.121027,-3.18514252],[3.16107488,-0.4239784],[3.05573153,2.90878654]]},{""UUID"":""24A8F474833744DFB02C488A9BA111E8"",""SemanticClassifications"":[""CEILING""],""Transform"":{""Translation"":[-1.06952024,3,1.2340889],""Rotation"":[90,93.14846,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-3.16107464,-3.18514252],""Max"":[3.16107464,3.18514252]},""PlaneBoundary2D"":[[-3.05573153,2.90878654],[-3.16107488,-0.423978329],[-3.121027,-3.18514252],[0.00542993844,-3.17527819],[-0.0159805864,-0.397333622],[3.16107464,-0.628885269],[3.1610744,3.18514228],[0.614610434,3.14264679]]},{""UUID"":""91C4D6D7094448EEA20403AE86EE6EC1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.71035528,1.5,1.268393],""Rotation"":[0,248.437622,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.8823391,-1.5],""Max"":[0.8823391,1.5]},""PlaneBoundary2D"":[[-0.8823391,-1.5],[0.8823391,-1.5],[0.8823391,1.5],[-0.8823391,1.5]]},{""UUID"":""33800E8EB9EE4C138FDE577C993EA90B"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.3914528,1.5,2.15365982],""Rotation"":[0,183.720367,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.9967317,-1.5],""Max"":[0.9967317,1.5]},""PlaneBoundary2D"":[[-0.9967317,-1.5],[0.9967317,-1.5],[0.9967317,1.5],[-0.9967317,1.5]]},{""UUID"":""A6282AA1CE7B446388350D3D92A48848"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-0.9457869,1.5,1.72746456],""Rotation"":[0,124.913544,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.5986103,-1.5],""Max"":[0.5986103,1.5]},""PlaneBoundary2D"":[[-0.5986103,-1.5],[0.5986103,-1.5],[0.5986103,1.5],[-0.5986103,1.5]]},{""UUID"":""EE8EAAA0CEC14186B1FF44A9DF52A2B1"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.49773419,1.5,-0.343039751],""Rotation"":[0,97.54905,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.59344471,-1.5],""Max"":[1.59344471,1.5]},""PlaneBoundary2D"":[[-1.59344471,-1.5],[1.59344471,-1.5],[1.59344471,1.5],[-1.59344471,1.5]]},{""UUID"":""F822A765695745529594E95CA5B92E8C"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[-1.22031093,1.5,-1.98077047],""Rotation"":[0,6.806239,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-0.4902168,-1.5],""Max"":[0.4902168,1.5]},""PlaneBoundary2D"":[[-0.4902168,-1.5],[0.4902168,-1.5],[0.4902168,1.5],[-0.4902168,1.5]]},{""UUID"":""4D96ABFBDD3744EDB7BF1417081005FD"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[0.6066754,1.5,-2.05464935],""Rotation"":[0,0.674668849,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.34031725,-1.5],""Max"":[1.34031725,1.5]},""PlaneBoundary2D"":[[-1.34031725,-1.5],[1.34031725,-1.5],[1.34031725,1.5],[-1.34031725,1.5]]},{""UUID"":""799EB9126BD94EFDA93418DDBEB28B99"",""SemanticClassifications"":[""WALL_FACE""],""Transform"":{""Translation"":[1.99076307,1.5,-0.8113152],""Rotation"":[0,271.995178,0],""Scale"":[1,1,1]},""PlaneBounds"":{""Min"":[-1.25988054,-1.5],""Max"":[1.25988054,1.5]},""PlaneBoundary2D"":[[-1.25988054,-1.5],[1.25988054,-1.5],[1.25988054,1.5],[-1.25988054,1.5]]},{""UUID"":""6853E03AE54E4DE9A82D8AC151BF00AC"",""SemanticClassifications"":[""OTHER""],""Transform"":{""Translation"":[0.95,1.0874939,1.63],""Rotation"":[270,5.721004,0],""Scale"":[1,1,1]},""VolumeBounds"":{""Min"":[-0.398986936,-0.432495117,-1.08197021],""Max"":[0.398986936,0.432495117,0]}}]}]}";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages\\com.meta.xr.mrutilitykit\\Tests\\CRUDTests.unity",
                new LoadSceneParameters(LoadSceneMode.Additive));
            yield return new WaitUntil(() => MRUK.Instance.IsInitialized);
            _currentRoom = MRUK.Instance.GetCurrentRoom();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (int i = SceneManager.sceneCount - 1; i >= 1; i--)
            {
                var asyncOperation =
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).name); // Clear/reset scene
                yield return new WaitUntil(() => asyncOperation.isDone);
            }
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator VerifyStartFromJson()
        {
            Assert.AreEqual(12, _currentRoom.Anchors.Count);
            Debug.Log(_currentRoom);
            yield return null;
        }
        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TwoAnchorsLess()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);
            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            int counterAnchorsOldRoom = _currentRoom.Anchors.Count;

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1LessAnchors);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");

            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(2, counterAnchorDeleted);
            Assert.AreEqual(0, counterAnchorUpdated);

            Assert.AreEqual(counterAnchorsOldRoom, _currentRoom.Anchors.Count + counterAnchorDeleted);


            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator TwoNewAnchors()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            int counterAnchorsUpdate = _currentRoom.Anchors.Count;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);
            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);
            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1MoreAnchors);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");

            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(2, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(0, counterAnchorUpdated);
            Assert.AreEqual(counterAnchorsUpdate, _currentRoom.Anchors.Count - counterAnchorCreated);


            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomOrderSwitched()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1Room3);

            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);


            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom3Room1);
            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");

            Assert.AreEqual(0, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(0, counterAnchorUpdated);

            yield return null;
        }


        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomAnchorPlaneBoundaryChanged()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1SceneAnchorPlaneBoundaryChanged);
            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");

            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(1, counterAnchorUpdated);

            yield return null;
        }


        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomAnchorVolumeBoundsChanged()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1SceneAnchorVolumeBoundsChanged);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");


            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(1, counterAnchorUpdated);

            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomAnchorPlaneRectChanged()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1SceneAnchorPlaneRectChanged);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");


            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(1, counterAnchorUpdated);

            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomAnchorLabelChanged()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom1SceneAnchorLabelChanged);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");


            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(1, counterAnchorUpdated);

            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator RoomUUIDChanged()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithScene1NewRoomGUID);
            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");


            Assert.AreEqual(1, counterRoomUpdated);
            Assert.AreEqual(0, counterRoomCreated);
            Assert.AreEqual(0, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(0, counterAnchorDeleted);
            Assert.AreEqual(0, counterAnchorUpdated);

            yield return null;
        }

        [UnityTest]
        [Timeout(DefaultTimeoutMs)]
        public IEnumerator Room2Loaded()
        {
            int counterRoomUpdated = 0;
            int counterRoomDeleted = 0;
            int counterRoomCreated = 0;

            int counterAnchorUpdated = 0;
            int counterAnchorDeleted = 0;
            int counterAnchorCreated = 0;
            MRUK.Instance.RegisterRoomUpdatedCallback(room => counterRoomUpdated++);
            MRUK.Instance.RegisterRoomCreatedCallback(room => counterRoomCreated++);
            MRUK.Instance.RegisterRoomRemovedCallback(room => counterRoomDeleted++);

            _currentRoom.RegisterAnchorCreatedCallback(anchor => counterAnchorCreated++);
            _currentRoom.RegisterAnchorRemovedCallback(anchor => counterAnchorDeleted++);
            _currentRoom.RegisterAnchorUpdatedCallback(anchor => counterAnchorUpdated++);

            MRUK.Instance.LoadSceneFromJsonString(SceneWithRoom2);

            Debug.Log($"counterRoomUpdated {counterRoomUpdated} counterRoomDeleted {counterRoomDeleted} counterRoomCreated {counterRoomCreated} " +
                      $"counterAnchorUpdated {counterAnchorUpdated} counterAnchorDeleted {counterAnchorDeleted} counterAnchorCreated {counterAnchorCreated}");

            Assert.AreEqual(0, counterRoomUpdated);
            Assert.AreEqual(1, counterRoomCreated);
            Assert.AreEqual(1, counterRoomDeleted);
            Assert.AreEqual(0, counterAnchorCreated);
            Assert.AreEqual(12, counterAnchorDeleted);
            Assert.AreEqual(0, counterAnchorUpdated);

            yield return null;
        }

    }
}
