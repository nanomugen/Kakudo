using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BotController : MonoBehaviour{

    [SerializeField]
    private float Speed=3F;
    [SerializeField]
    private float JumpForce=5F;
    [SerializeField]
    private float CameraHeight=1f;
    [SerializeField]
    private float CameraDistance=3.5f;
    [SerializeField]
    private float CameraMoveMin=0.15f;
    [SerializeField]
    private float Sensibility=10f;
    [SerializeField]
    private float SensibilityFPS=2f;


    private Rigidbody rigidbody;
    private bool ground;
    private bool isJumping;
    private Animator animator;

    private bool MenuOpened;
    private bool MessageSpawn;
    private float MessageCount;
    private float MessageTimeout=15f;
    private string Message;
    private GUIStyle guiStyle;

    private Texture2D lastPic;
    //private Directory Pictures;

    private float AngleX=0f;
    private float AngleY=0f;
    private float XLimit;//limit to 1P mouse x move(to see the y quartenion at camera localRotation)
    private Vector3 CameraPosition3P;
    private Vector3 CameraPosition1P;
    private Vector3 Center1P;
    private float Zoom;
    private bool FirstPerson;

    //sound
    private AudioClip walkSound;
    private AudioSource audioScr;

    
    //MENU (WHEN PRESS ESC)
    void OnGUI(){
        
        if(MenuOpened){
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
                GUILayout.Label("CameraHeight: "+CameraHeight);
                //CameraHeight = GUILayout.HorizontalSlider(CameraHeight,2f,10f);
                GUILayout.Label("CameraDistance: "+CameraDistance);
                //CameraDistance = GUILayout.HorizontalSlider(CameraDistance,2f,10f);
                GUILayout.Label("Mouse Sensibility: "+Sensibility);
                Sensibility = GUILayout.HorizontalSlider(Sensibility,5f,50f);
                GUILayout.Label("Mouse Sensibility FPS: "+SensibilityFPS);
                SensibilityFPS = GUILayout.HorizontalSlider(SensibilityFPS,0.5f,8f);
                GUILayout.Label("Speed: "+Speed);
                Speed = GUILayout.HorizontalSlider(Speed,2f,10f);
                GUILayout.Label("Jump: "+JumpForce);
                JumpForce = GUILayout.HorizontalSlider(JumpForce,2f,10f);
                GUILayout.Label("Message Timeout: "+ MessageTimeout);
                MessageTimeout = GUILayout.HorizontalSlider(MessageTimeout,1f,30f);
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(10,Screen.height-30,50,20));
                if(GUILayout.Button("Quit")){
                    Application.Quit();
                }
            GUILayout.EndArea();
        }
        if(MenuOpened){//colocar uma variavel aqui pra mostrar quando tirar a foto tambem
            GUILayout.BeginArea(new Rect(10,Screen.height-200,160,90));
                if(lastPic!=null){
                    GUILayout.Box(lastPic);
                }
            GUILayout.EndArea();
        }
        if(MessageSpawn){
            if(MessageCount<=MessageTimeout){
                //GUILayout.BeginArea(new Rect((Screen.width/2)-240,Screen.height-80,480,40));
                GUI.backgroundColor = Color.black;
                GUI.Box(new Rect((Screen.width/2)-240,Screen.height-80,480,40),Message,guiStyle);
                //GUILayout.EndArea();
                MessageCount+=Time.deltaTime;
            }
            else{
                MessageSpawn=false;
            }
            
        }
        
    }
    void MessageStart(string msg){
        Message = msg;
        MessageCount=0f;
        MessageSpawn=true;
    }
    

    // Start is called before the first frame update
    void Start(){
        Vector3 cameraDir = new Vector3(1f,0f,-1f).normalized;
        CameraPosition3P= transform.position+cameraDir*CameraDistance+new Vector3(0f,CameraHeight,0f);
        Camera.main.transform.position = CameraPosition3P;
        transform.LookAt(new Vector3(Camera.main.transform.position.x,transform.position.y,Camera.main.transform.position.z));
        rigidbody = gameObject.GetComponent<Rigidbody>();
        animator = gameObject.GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        audioScr = gameObject.GetComponent<AudioSource>();
        walkSound = Resources.Load<AudioClip>("Sounds/botwalk");
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 32;
        guiStyle.normal.textColor = Color.white;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        //lastPic = Resources.Load<Texture>("screenshot.png");
        Directory.CreateDirectory("Pictures");
        lastPic = new Texture2D(Screen.width,Screen.height,TextureFormat.ARGB32,false);
        //if(lastPic.LoadImage(File.ReadAllBytes("Pictures"+Path.DirectorySeparatorChar+"screenshot.png"))){
        //    Debug.Log("loaded");
        //}
        //IMPLEMENT A SYSTEM THAT SAVE THE NAME OF LAST PICTURE************************

        

    }

    // Update is called once per frame
    void Update(){
        //Move();
        //Jump();
        //Menu();
        //CameraMove();
        //Freeze();
        //Debug.Log("B-POS: "+transform.position+"C-POS: "+Camera.main.transform.position+"C-FOR: "+Camera.main.transform.forward);
        
    }
    void LateUpdate() {
        Move();
        Jump();
        Menu();
        CameraMove();
        Freeze();
        Snap();
    }

    //fazer salvar multiplas screenshots com (1) (2) etc ou com o tempo(data+segundos do dia)
    void Snap(){
        if(FirstPerson && Input.GetButtonDown("LeftClick")&&!MenuOpened){
            Debug.Log("take snap "+ System.DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            lastPic=ScreenCapture.CaptureScreenshotAsTexture(1);
            File.WriteAllBytes("Pictures"+Path.DirectorySeparatorChar+"screenshot"+System.DateTime.Now.ToString("yyyyMMddHHmmssffff")+".png",lastPic.EncodeToPNG());
            MessageStart("You Took a Picture!");
            
        }
    }

    void OnPostRender() {
        
    }
    

    void CameraMove(){
        if(!MenuOpened){
            if((Input.GetAxisRaw("LT")>=0.8f||Input.GetKey(KeyCode.Tab)) && !FirstPerson){
                FirstPerson=true;
                CameraPosition1P = transform.position+new Vector3(0f,1f,0f)+transform.forward;
                Center1P = CameraPosition1P;
                Zoom=0f;
                Camera.main.transform.position = CameraPosition1P;
                Camera.main.transform.forward=transform.forward;
                //Debug.Log("Camera position: "+ Camera.main.transform.position+" Camera.localRotation: "+Camera.main.transform.localRotation.eulerAngles);
                XLimit = Camera.main.transform.localRotation.eulerAngles.y;
                AngleY=XLimit;
                AngleX=0f;
                animator.SetBool("WALK",false);
                animator.SetBool("JUMP",false);
                if(audioScr.isPlaying && audioScr.clip==walkSound){
                    audioScr.Stop();
                    audioScr.loop=false;
                }
            }
            else{
                if((Input.GetAxisRaw("LT")<=0.3f && !Input.GetKey(KeyCode.Tab)) && FirstPerson){
                    FirstPerson=false;
                    Camera.main.transform.position = CameraPosition3P; 
                }
            }
            if(FirstPerson){
                //Debug.Log("")
                float sideMovement = Input.GetAxis("Horizontal");
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                Zoom = Mathf.Clamp(Zoom+scroll,0f,2f);
                CameraPosition1P += 0.1f*(transform.right*sideMovement);
                if(Vector3.Distance(CameraPosition1P,Center1P)>2f){//DEFINIR RAIO DE DISTANCIA MÁXIMA#####
                    CameraPosition1P =Center1P + 2*((CameraPosition1P-Center1P).normalized);
                }
                Camera.main.transform.position = CameraPosition1P+Zoom*transform.forward;
                AngleX += -Input.GetAxis("Mouse Y")*SensibilityFPS;
                AngleY += Input.GetAxis("Mouse X")*SensibilityFPS;
                AngleX = Mathf.Clamp(AngleX,-50f,50f);
                AngleY = Mathf.Clamp(AngleY,XLimit-50f,XLimit+50f);
                Camera.main.transform.localRotation = Quaternion.Euler(AngleX,AngleY,0f);
                //Debug.Log("Camera position: "+ Camera.main.transform.position+" Camera.localRotation: "+Camera.main.transform.localRotation.eulerAngles);
            }
        }
    }
    void Move(){
        if(!FirstPerson && !MenuOpened){
            float sideMovement = Input.GetAxisRaw("Horizontal");
            float frontMovement = Input.GetAxisRaw("Vertical");
            Vector3 movementVector = new Vector3(sideMovement,0f,frontMovement);
            
            //front -> vector that point to the front of player based on the camera
            Vector3 front = (transform.position-new Vector3(Camera.main.transform.position.x,transform.position.y,Camera.main.transform.position.z)).normalized;
            //side -> vector that point to the right(?) direction of the body, ortogonal to the front and up vector
            Vector3 side = Vector3.Cross(Vector3.up,front).normalized;
            Vector3 SideVelocity = side*sideMovement;//*Time.deltaTime;
            Vector3 FrontVelocity = front*frontMovement;//*Time.deltaTime;
            transform.LookAt(transform.position + (SideVelocity+FrontVelocity));
            Vector3 velocity = Speed*(SideVelocity+FrontVelocity);
            rigidbody.velocity = new Vector3(velocity.x,rigidbody.velocity.y,velocity.z);
            if(movementVector.magnitude >0.3f){    
                animator.SetBool("WALK",true);
                if(!audioScr.isPlaying&&ground){
                    audioScr.clip = walkSound;
                    audioScr.loop=true;
                    audioScr.Play();
                }
            }
            else{
                if(rigidbody.velocity.y >0 && ground && !isJumping){
                    rigidbody.velocity = new Vector3(0f,0f,0f);
                }
                animator.SetBool("WALK",false);
                if(audioScr.isPlaying && audioScr.clip==walkSound){
                    audioScr.Stop();
                    audioScr.loop=false;
                }
            }
            float sideMouse = Input.GetAxisRaw("Mouse X");
            float upMouse = Input.GetAxisRaw("Mouse Y");
            //Debug.Log(Input.GetAxisRaw("Mouse ScrollWheel"));
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if(sideMouse > CameraMoveMin || sideMouse < -CameraMoveMin){
                Vector3 sideCamera = Vector3.Cross(transform.position - Camera.main.transform.position,Vector3.up).normalized;
                CameraPosition3P += sideCamera*sideMouse*Sensibility*Time.deltaTime;//*(-1);
                Camera.main.transform.position = CameraPosition3P;
                
            }
            if((upMouse < -CameraMoveMin && CameraHeight <=3f) || (upMouse > CameraMoveMin && CameraHeight >=0.5f)){
                CameraHeight-=upMouse*Time.deltaTime*Sensibility;
            }
            if((scroll>=0.1f && CameraDistance >=3f)||(scroll<=-0.1 && CameraDistance <=10f)){
                CameraDistance-=scroll;
            }
            
            front = (transform.position-new Vector3(Camera.main.transform.position.x,transform.position.y,Camera.main.transform.position.z)).normalized;
            side = Vector3.Cross(Vector3.up,front).normalized;
            CameraPosition3P = transform.position+CameraDistance*(-1)*front+new Vector3(0f,CameraHeight,0f);//NESSA FUNÇÃO ACOMPANHA SEMPRE CENTRALIZADO, QUANDO PULA A CAMERA MEXE
            Camera.main.transform.position = CameraPosition3P;
            //Camera.main.transform.position = new Vector3(transform.position.x,0f,transform.position.z)+CameraDistance*(-1)*front+new Vector3(0f,CameraHeight,0f);//NESSA FUNÇÃO A CAMERA ESTÁ SEMPRE NA MESMA ALTURA, O PULO FICA MELHOR, A CAMERA NAO MEXE AO PULAR
            
            Camera.main.transform.LookAt(transform.position);
        }
    }
    void Jump(){
        if(Input.GetButtonDown("B")&&ground&&!MenuOpened&&!FirstPerson){
            isJumping=true;//POR CAUSA DA RAMPA
            rigidbody.velocity += Vector3.up*JumpForce;
        }
    }
    void Menu(){

        if(Input.GetKeyDown(KeyCode.Escape)){
            if(MenuOpened){
                Cursor.lockState = CursorLockMode.Locked;
                MenuOpened = false;
            }
            else{
                Cursor.lockState = CursorLockMode.None;
                MenuOpened = true;
            }
            //Debug.Log("MenuOpened: "+ MenuOpened + " Cursor.lockState: "+ Cursor.lockState);
        }
    }
    void Freeze(){

    }

    private void OnCollisionEnter(Collision other) {
        //6. floor
        if(other.gameObject.layer == 6){
            isJumping=false;//SÓ POR CAUSA DA RAMPA
            ground =true;
            animator.SetBool("JUMP",false);
        }
    }

    private void OnCollisionStay(Collision other) {
        //6. floor
        if(other.gameObject.layer == 6){
            //Debug.Log("floor collision enter");
            ground =true;
            animator.SetBool("JUMP",false);
            ContactPoint contact = other.contacts[0];
            //Debug.Log("Player: "+ transform.position+ " Collision: "+contact.point);//fazer comparação com ambos para definir quando pusou de fato
        }
    }
    private void OnCollisionExit(Collision other) {
        //6. floor
        if(other.gameObject.layer == 6){
            ground =false;
            animator.SetBool("JUMP",true);
            if(audioScr.isPlaying && audioScr.clip==walkSound){
                audioScr.Stop();
                audioScr.loop=false;
            }
        }
    }

}
