using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryTestsController : MonoBehaviour
{
    [SerializeField]
    private Button[] buttons = null;

    private MemoryTestsBase[] tests;

    private void Awake()
    {
        tests = gameObject.GetComponentsInChildren<MemoryTestsBase>();
        for (int i =0; i < buttons.Length; i++)
        {
            if (i < tests.Length)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].name = i.ToString();
                buttons[i].GetComponentInChildren<Text>().text = tests[i].GetType().Name;
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < tests.Length; i++)
        {
            buttons[i].interactable = !tests[i].enabled;
        }
    }

    public void OnClickButton(GameObject button)
    {
        int index = int.Parse(button.name);
        tests[index].enabled = true;
    }
}
