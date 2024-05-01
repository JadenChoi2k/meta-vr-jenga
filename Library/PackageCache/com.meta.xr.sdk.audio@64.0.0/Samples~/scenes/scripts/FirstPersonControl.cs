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

using UnityEngine;

public class FirstPersonControl : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float mouseSensitivity = 5.0f;
    public float headHeight = 1.5f;

    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime, 0.0f,
            Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime);

        Vector3 pos = transform.position;
        pos.y = headHeight;
        transform.position = pos;

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity, Space.World);
            transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * mouseSensitivity, Space.Self);
        }
    }
}
