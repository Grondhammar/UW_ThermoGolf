using UnityEngine;
using System.Collections;

public class Button_PlayGame : MonoBehaviour
{
	void OnClick()
	{
		Application.LoadLevel ("GameScreen");
	}
}
