using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MartialGameManager : MonoBehaviour
{
    private GameState state = GameState.SelectAction;
    private GameObject selectActionUi;

    public MartialCharacter player;

    private Transform pointer;
    // Start is called before the first frame update
    void Start()
    {
        selectActionUi = GameObject.Find("SelectActionUI");
        pointer = GameObject.Find("Pointer").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!player.GetComponent<Animator>().GetBool("IsMoving"))
        {
            if (Physics.Raycast(ray, out var hit, 100f, 1 << LayerMask.NameToLayer("MartialFloor")))
            {
                pointer.position = hit.point;

                if (state == GameState.SetMovePosition)
                {
                    player.transform.LookAt(pointer.position);
                    player.shadow.rotation = player.transform.rotation;
                    player.shadow.position = player.transform.position;
                    var distance = Mathf.Min(Mathf.Sqrt(player.MaxMoveAmount), (pointer.position - player.transform.position).magnitude);
                    player.shadow.position += player.transform.forward * distance;
                }
                else if (state == GameState.SetRotation)
                {
                    player.shadow.LookAt(pointer.position);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (state == GameState.SetMovePosition)
            {
                state = GameState.SetRotation;
                player.HideMoveRange();
                player.ShowAttackRange();
            }
            else if (state == GameState.SetRotation)
            {
                state = GameState.Move;
                player.HideShadow();
                player.HideAttackRange();
                player.GetComponent<Animator>().SetBool("IsMoving", true);

                var rotation = player.shadow.rotation;
                var end = new Vector3(player.shadow.position.x, 0, player.shadow.position.z);
                var start = new Vector3(player.transform.position.x, 0, player.transform.position.z);
                var distance = (end - start).magnitude;
                player.rb.velocity = player.transform.forward * distance;
                StartCoroutine(aaa());
                IEnumerator aaa()
                {
                    yield return new WaitForSeconds(1);
                    player.rb.velocity = Vector3.zero;
                    player.transform.rotation = rotation;

                    player.GetComponent<Animator>().SetBool("IsMoving", false);
                    state = GameState.SelectAction;
                    selectActionUi.SetActive(true);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (state == GameState.SetMovePosition)
            {
                state = GameState.SelectAction;
                selectActionUi.SetActive(true);
                player.HideMoveRange();
            }
            if (state == GameState.SetRotation)
            {
                state = GameState.SetMovePosition;
                player.HideAttackRange();
                player.ShowMoveRange();
            }
        }
    }

    public void OnMoveButtonClick()
    {
        if (state == GameState.SelectAction)
        {
            state = GameState.SetMovePosition;
            selectActionUi.SetActive(false);
            player.ShowMoveRange();
            player.ShowShadow();
        }
    }

    private enum GameState
    {
        SelectAction,
        SetMovePosition,
        SetRotation,
        PreMove,
        Move,
        PostMove,
    }
}
