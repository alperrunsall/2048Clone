using UnityEngine;
using TMPro;
using System.Collections;

[SelectionBase]
public class Cube : MonoBehaviour
{
    public int value;
    public TextMeshProUGUI text;

    private readonly int ANIM_STATE = Animator.StringToHash("State");

    public void SetAnimState(int state)
    {
        GetComponent<Animator>().SetInteger(ANIM_STATE, state);
    }
    public int GetAnimState()
    {
        return GetComponent<Animator>().GetInteger(ANIM_STATE);
    }


    public IEnumerator HoverCor(GameObject cube, int value)
    {
        Material mat = cube.GetComponent<Renderer>().material;
        float newDissolveValue = value == 1 ? 0f : 1f;

        if (value == 0)
        {
            while (newDissolveValue > 0.0f)
            {
                newDissolveValue -= Time.deltaTime * 5f;
                mat.SetFloat("_DissolveValue", newDissolveValue);
                yield return null;
            }
        }
        else
        {
            while (newDissolveValue < 1.0f)
            {
                newDissolveValue += Time.deltaTime * 5f;
                mat.SetFloat("_DissolveValue", newDissolveValue);
                yield return null;
            }
        }
    }
}
