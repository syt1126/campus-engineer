using System.Collections.Generic;
using UnityEngine;

namespace CampusEngineer
{
    // Deliberately restrained gestures for a friendly guide, with no sudden approach or stare.
    public class CampusGuideMotion:MonoBehaviour
    {
        public Transform target;
        Transform head,spine,leftArm,rightArm,leftForearm,rightForearm;
        Quaternion headRest,spineRest,leftRest,rightRest,leftForeRest,rightForeRest;
        readonly List<SkinnedMeshRenderer> faces=new List<SkinnedMeshRenderer>();
        float start;
        Transform Bone(string name){foreach(var t in GetComponentsInChildren<Transform>())if(t.name=="Human_"+name)return t;return null;}
        void Start()
        {
            head=Bone("head");spine=Bone("spine");leftArm=Bone("leftUpperArm");rightArm=Bone("rightUpperArm");leftForearm=Bone("leftLowerArm");rightForearm=Bone("rightLowerArm");
            Relax(leftArm,-.14f);Relax(rightArm,.14f);Relax(leftForearm,-.1f);Relax(rightForearm,.1f);
            if(head!=null)headRest=head.localRotation;if(spine!=null)spineRest=spine.localRotation;
            if(leftArm!=null)leftRest=leftArm.localRotation;if(rightArm!=null)rightRest=rightArm.localRotation;
            if(leftForearm!=null)leftForeRest=leftForearm.localRotation;if(rightForearm!=null)rightForeRest=rightForearm.localRotation;
            foreach(var face in GetComponentsInChildren<SkinnedMeshRenderer>())if(face.sharedMesh.blendShapeCount>0)faces.Add(face);
            start=Time.time;
        }
        void Relax(Transform bone,float side)
        {
            if(bone==null || bone.childCount==0)return;Vector3 direction=bone.GetChild(0).position-bone.position;
            if(direction.sqrMagnitude>.00001f)bone.rotation=Quaternion.FromToRotation(direction.normalized,transform.TransformDirection(new Vector3(side,-1,-.07f)).normalized)*bone.rotation;
        }
        void LateUpdate()
        {
            float t=Time.time-start;float angle=0;
            if(target!=null){var d=transform.InverseTransformDirection(target.position-transform.position);angle=Mathf.Clamp(Mathf.Atan2(-d.x,-d.z)*Mathf.Rad2Deg,-14,14);}
            if(head!=null)head.localRotation=Quaternion.Slerp(head.localRotation,headRest*Quaternion.Euler(Mathf.Sin(t*.7f)*1.2f,angle*.35f,Mathf.Sin(t*.4f)*1.3f),Time.deltaTime*2);
            if(spine!=null)spine.localRotation=spineRest*Quaternion.Euler(Mathf.Sin(t*1.3f)*.6f,0,Mathf.Sin(t*.45f)*.6f);
            float g=t%16,gesture=g>5 && g<8?Mathf.Sin((g-5)/3*Mathf.PI):0;
            if(leftArm!=null)leftArm.localRotation=leftRest*Quaternion.Euler(Mathf.Sin(t*.8f)*1.2f,0,0);
            if(rightArm!=null)rightArm.localRotation=rightRest*Quaternion.Euler(Mathf.Sin(t*.7f)*1.1f,gesture*6,gesture*8);
            if(leftForearm!=null)leftForearm.localRotation=leftForeRest*Quaternion.Euler(0,0,Mathf.Sin(t*.6f));
            if(rightForearm!=null)rightForearm.localRotation=rightForeRest*Quaternion.Euler(gesture*12,0,0);
            float cycle=t%4.8f,blink=cycle<.18f?Mathf.Sin(cycle/.18f*Mathf.PI)*100:0;
            foreach(var face in faces)
            {
                // Names are assigned from the VRM expression binds by the data preparation tool.
                for(int i=0;i<face.sharedMesh.blendShapeCount;i++)
                {string n=face.sharedMesh.GetBlendShapeName(i);if(n=="Blink")face.SetBlendShapeWeight(i,blink);if(n=="Joy")face.SetBlendShapeWeight(i,12);}
            }
        }
    }
}
