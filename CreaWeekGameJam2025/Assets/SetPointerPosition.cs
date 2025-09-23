using UnityEngine;

public class SetPointerPosition : MonoBehaviour
{

    [SerializeField] Transform Player, Leader;
    [SerializeField] Camera Camera;

    [SerializeField] float offset = 40;

    private void Start()
    {
        Camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (Camera.WorldToViewportPoint(Leader.position) - Camera.WorldToViewportPoint(Player.position));

        Vector3 newPos = new Vector3(dir.x, dir.y, 0).normalized * 2000;
        newPos.x = Mathf.Clamp(newPos.x, (-1920 / 2) + offset, (1920 / 2) - offset); 
        newPos.y = Mathf.Clamp(newPos.y, (-1080 / 2) + offset, (1080 / 2) - offset);

        transform.localPosition = newPos;

        transform.GetChild(0).gameObject.SetActive(!(Camera.WorldToViewportPoint(Leader.position).x > .1f && Camera.WorldToViewportPoint(Leader.position).x < .9f && Camera.WorldToViewportPoint(Leader.position).y > .1f && Camera.WorldToViewportPoint(Leader.position).y < .9f));
        
        
        
    }
}
