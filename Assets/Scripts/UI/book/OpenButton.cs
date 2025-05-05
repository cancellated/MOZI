using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenButton:MonoBehaviour
{
    [SerializeField] private GameObject openObject;
    public void Open(){
        if(openObject != null){
            openObject.SetActive(true);
        }
    }
    public void Close(){
        if(openObject!= null){
            openObject.SetActive(false);
        }
    }

    [System.Obsolete]
    public void OpenOrClose(){
        if(openObject != null){
            if(openObject.active == true){
            openObject.SetActive(false);
        }else{
            openObject.SetActive(true);
        }
        }
        else{
            Debug.Log("影神图不存在");
        }
    }
}
