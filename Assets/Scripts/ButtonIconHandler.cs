using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonIconHandler : MonoBehaviour
{
    public string ID, type;
    public bool isClicked;
    public Sprite frontImage, backImage;

    public void OnClick()
    {
        if (!isClicked && GameHandler.canClick)
        {
            GetComponent<Image>().sprite = frontImage;
            isClicked = true;

            if (GameHandler.iconPair1 == null)
            {
                GameHandler.iconPair1 = this.gameObject;
            } else
            {
                if (GameHandler.iconPair2 == null)
                {
                    GameHandler.iconPair2 = this.gameObject;
                    GameHandler.singleton.checkPair();
                }
            }
        }
    }
}
