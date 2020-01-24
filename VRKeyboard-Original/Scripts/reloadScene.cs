using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class reloadScene : MonoBehaviour
{
    #region Public Variables
    public OVRInput.RawButton reloadButton;
    public float waitTime = 1f;
    #endregion
    
    #region Private Variables
    private bool reloadInitiated = false;
    #endregion

    private void Update()
    {
        if (OVRInput.Get(reloadButton) && !reloadInitiated)
        {
            reloadInitiated = true;
            StartCoroutine(sceneLoading());
        }
    }

    private IEnumerator sceneLoading()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}





