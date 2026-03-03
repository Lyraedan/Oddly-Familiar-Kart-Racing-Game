using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float offset; // we need to change this to a lesser value whenever there is antigravity AND the race is completed

    public Vector3 boost_pos = new Vector3(0, 1.24f, -6.5f);
    public Vector3 orig_pos;
    public Vector3 bulletPos;
    public float antiGravityPosY;

    [HideInInspector]
    public float antiGravityTimeAgo = 0;
    [HideInInspector]
    public bool rotateCamAntiGravity = false;
    [HideInInspector]
    public float rotateAmountAntigravityX = 0;
    public float rotateAmountAntigravityZ = 0;

    [HideInInspector]
    public float glideAngleZ;
    public float glideAngleX = 0;

    public float raceEndFOV = 0;
    float none = 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        Player player = RaceManager.Instance.LocalPlayer;
        if (player == null)
            return;

        if (!player.outOfBounds.PlayerBeingMoved)
        {
            antiGravityTimeAgo += Time.deltaTime;

            //anti gravity vs regular position
            if (player.antiGravity || antiGravityTimeAgo < 3 || player.GLIDER_FLY)
            {
                Ray upRay = new Ray(player.transform.position, player.transform.up);

                Vector3 upDist;
                if (!RaceManager.RACE_COMPLETED)
                {
                    upDist = upRay.GetPoint(offset);
                }
                else
                {
                    upDist = upRay.GetPoint(offset-0.8f);
                }
                transform.position = upDist;
            }
            else
            {

                {
                    transform.position = player.transform.position + new Vector3(0, offset, 0);
                }
            }

            if (!player.GLIDER_FLY && !player.trickBoostPending && RaceManager.RACE_STARTED)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, player.transform.rotation, 3f * Time.deltaTime);
            }
            else
            {

                    float angle = transform.localEulerAngles.x;
                    angle = (angle > 180) ? angle - 360 : angle;

                if (player.GLIDER_FLY)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(glideAngleX, player.transform.eulerAngles.y, glideAngleZ), 1 * Time.deltaTime);
                }
                else if(player.trickBoostPending && !player.antiGravity)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, player.transform.eulerAngles.y, 0), 3 * Time.deltaTime);
                }



            }

            if (player.JUMP_PANEL)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, player.transform.rotation, 0.4f * Time.deltaTime);
            }

            //rotation antigravity
            if (player.antiGravity)
            {
                if (rotateCamAntiGravity)
                {
                    transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, Quaternion.Euler(rotateAmountAntigravityX, transform.GetChild(0).localEulerAngles.y, rotateAmountAntigravityZ), 3 * Time.deltaTime);
                }
                else
                {
                    transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, Quaternion.Euler(2, transform.GetChild(0).localEulerAngles.y, 0), 3 * Time.deltaTime);
                }
            }
            else
            {
                transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, Quaternion.Euler(2, transform.GetChild(0).localEulerAngles.y, 0), 3 * Time.deltaTime);
            }


            if ((player.Boost || player.trickBoostPending) && !player.itemManager.isBullet && !RaceManager.RACE_COMPLETED)
            {
                if (!rotateCamAntiGravity)
                    transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, boost_pos, 4f * Time.deltaTime);
                else
                {
                    transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(boost_pos.x, antiGravityPosY, boost_pos.z), 4f * Time.deltaTime);
                }
            }
            if (!player.Boost && !player.itemManager.isBullet)
            {
                if (!rotateCamAntiGravity)
                    transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, orig_pos, 4f * Time.deltaTime);
                else
                {
                    transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(orig_pos.x, antiGravityPosY, orig_pos.z), 4f * Time.deltaTime);
                }
            }
            if (RaceManager.RACE_COMPLETED)
            {
                transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, orig_pos, 3 * Time.deltaTime);
            }
            if (player.itemManager.isBullet && !RaceManager.RACE_COMPLETED)
            {
                transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, bulletPos, 6 * Time.deltaTime);
            }
        }

        if (RaceManager.RACE_COMPLETED)
        {
            transform.localScale = new Vector3(1, 1, 1);
            if(raceEndFOV > 1)
            {
                transform.GetChild(0).GetChild(0).GetComponent<Camera>().fieldOfView = raceEndFOV;
            }
        }
    }
}
