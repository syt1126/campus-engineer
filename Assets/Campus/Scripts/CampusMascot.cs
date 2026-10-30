using UnityEngine;

namespace CampusEngineer
{
    // Original campus character. All surfaces, expressions and motion are created locally.
    public sealed class CampusMascot:MonoBehaviour
    {
        public Transform target;
        public Transform head,leftArm,rightArm,tail,body;
        Transform[] eyes;
        Vector3 basePosition;
        float greetingUntil;
        public int Greetings {get;private set;}
        public bool Greeting {get{return Time.time<greetingUntil;}}
        public void Greet(){greetingUntil=Time.time+2.4f;Greetings++;}
        public static CampusMascot Create(Transform parent,Vector3 position,Transform target)
        {return CampusCompanionModel.Create(parent,position,target);}
        internal void Configure(Transform viewer,Vector3 position,Transform[] eyelids){target=viewer;basePosition=position;eyes=eyelids;}
        void LateUpdate()
        {
            float t=Time.time;
            float wave=Greeting?Mathf.Sin(Mathf.Clamp01((greetingUntil-t)/2.4f)*Mathf.PI):0;
            transform.localPosition=basePosition+Vector3.up*(Mathf.Sin(t*1.7f)*.009f+Mathf.Abs(Mathf.Sin(t*5))*wave*.08f);
            body.localScale=new Vector3(1,1+Mathf.Sin(t*1.8f)*.012f,1);
            Vector3 d=target==null?Vector3.back:target.position+Vector3.up*1.3f-head.position;
            float yaw=Mathf.Clamp(Mathf.Atan2(-d.x,-d.z)*Mathf.Rad2Deg,-28,28);
            head.localRotation=Quaternion.Slerp(head.localRotation,Quaternion.Euler(Mathf.Sin(t)*2,yaw,Mathf.Sin(t*.6f)*3+wave*8),Time.deltaTime*4);
            rightArm.localRotation=Quaternion.Euler(wave*-40,0,wave*(130+Mathf.Sin(t*13)*20));
            leftArm.localRotation=Quaternion.Euler(Mathf.Sin(t*1.4f)*4,0,-wave*12);
            tail.localRotation=Quaternion.Euler(Mathf.Sin(t*1.1f)*7,Mathf.Sin(t*1.8f)*(12+wave*18),0);
            float blink=t%4.2f;float open=blink<.16f?1-Mathf.Sin(blink/.16f*Mathf.PI)*.94f:1;
            foreach(var eye in eyes)eye.localScale=new Vector3(1,open,1);
        }
    }
}
