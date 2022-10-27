using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : Button, IPointerDownHandler, IPointerUpHandler
{
    private bool pointerDown;
    private float pointerDownTimer;
    private float repeatTimer;

    private float heldBufferTime = 1f;
    private float repeatTime = 0.2f;

    private PointerEventData pointerEventData;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        pointerDown = true;
        pointerEventData = eventData;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        pointerDown = false;
        pointerDownTimer = 0;
    }

    private void Update()
    {
        if (pointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            repeatTimer += Time.deltaTime;

            if (pointerDownTimer >= heldBufferTime && repeatTimer >= repeatTime)
            {

                base.OnPointerClick(pointerEventData);

                repeatTimer = 0;
            }
        }
    }
}
