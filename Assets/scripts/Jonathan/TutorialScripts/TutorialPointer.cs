using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPointer : MonoBehaviour
{
    [Header("Pointer objects")]
    [SerializeField] private RectTransform magicHand;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Image tapRipple;

    [Header("Magic hand animation")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private float handTapScale = 0.85f;
    [SerializeField] private float handTapTime = 0.12f;
    [SerializeField] private float handTapDelay = 0.45f;

    [Header("Arrow animation")]
    [SerializeField] private bool arrowSpritePointsDownAtZero = true;
    [SerializeField] private float arrowBounceDistance = 12f;
    [SerializeField] private float arrowBounceDuration = 0.45f;

    private Sequence handTapSequence;
    private Sequence arrowBounceSequence;
    private Sequence rippleSequence;
    private Vector3 originalHandScale;
    private Vector3 originalArrowScale;

    private void Awake()
    {
        if (magicHand != null)
            originalHandScale = magicHand.localScale;

        if (arrow != null)
            originalArrowScale = arrow.localScale;

        HideAll();
    }

    public void HideAll()
    {
        KillTweens();

        if (magicHand != null)
            magicHand.gameObject.SetActive(false);

        if (arrow != null)
            arrow.gameObject.SetActive(false);

        if (tapRipple != null)
            tapRipple.gameObject.SetActive(false);
    }

    public void HideHand()
    {
        if (handTapSequence != null)
            handTapSequence.Kill();

        if (rippleSequence != null)
            rippleSequence.Kill();

        if (magicHand != null)
        {
            magicHand.DOKill();
            magicHand.localScale = originalHandScale;
            magicHand.gameObject.SetActive(false);
        }

        if (tapRipple != null)
            tapRipple.gameObject.SetActive(false);
    }

    public void HideArrow()
    {
        if (arrowBounceSequence != null)
            arrowBounceSequence.Kill();

        if (arrow != null)
        {
            arrow.DOKill();
            arrow.localScale = originalArrowScale;
            arrow.gameObject.SetActive(false);
        }
    }

    public void ShowMagicHand(RectTransform target, TutorialTargetPoint targetPoint, Vector2 extraOffset, bool playTap)
    {
        if (magicHand == null || target == null)
            return;

        HideArrow();

        Vector2 localPosition = GetTargetLocalPoint(target, magicHand.parent as RectTransform, targetPoint) + extraOffset;

        magicHand.gameObject.SetActive(true);
        magicHand.SetAsLastSibling();
        magicHand.DOKill();
        magicHand.DOAnchorPos(localPosition, moveDuration).SetEase(moveEase).OnComplete(() =>
        {
            if (playTap)
                StartHandTapLoop();
        });
    }

    public void ShowArrow(RectTransform target, TutorialArrowSide side, float distanceFromTarget, Vector2 extraOffset, float extraRotationZ, bool bounce)
    {
        if (arrow == null || target == null)
            return;

        HideHand();

        Vector2 localPosition = GetArrowLocalPoint(target, arrow.parent as RectTransform, side, distanceFromTarget) + extraOffset;

        arrow.gameObject.SetActive(true);
        arrow.SetAsLastSibling();
        arrow.localRotation = Quaternion.Euler(0f, 0f, GetArrowRotationZ(side) + extraRotationZ);
        arrow.DOKill();
        arrow.DOAnchorPos(localPosition, moveDuration).SetEase(moveEase).OnComplete(() =>
        {
            if (bounce)
                StartArrowBounce(side);
        });
    }

    private void StartHandTapLoop()
    {
        if (magicHand == null)
            return;

        if (handTapSequence != null)
            handTapSequence.Kill();

        magicHand.localScale = originalHandScale;

        handTapSequence = DOTween.Sequence();
        handTapSequence.AppendInterval(handTapDelay);
        handTapSequence.AppendCallback(PlayTapRipple);
        handTapSequence.Append(magicHand.DOScale(originalHandScale * handTapScale, handTapTime));
        handTapSequence.Append(magicHand.DOScale(originalHandScale, handTapTime));
        handTapSequence.SetLoops(-1);
    }

    private void StartArrowBounce(TutorialArrowSide side)
    {
        if (arrow == null)
            return;

        if (arrowBounceSequence != null)
            arrowBounceSequence.Kill();

        Vector2 startPosition = arrow.anchoredPosition;
        Vector2 offset = GetArrowBounceOffset(side, arrowBounceDistance);

        arrowBounceSequence = DOTween.Sequence();
        arrowBounceSequence.Append(arrow.DOAnchorPos(startPosition + offset, arrowBounceDuration).SetEase(Ease.InOutSine));
        arrowBounceSequence.Append(arrow.DOAnchorPos(startPosition, arrowBounceDuration).SetEase(Ease.InOutSine));
        arrowBounceSequence.SetLoops(-1);
    }

    private void PlayTapRipple()
    {
        if (tapRipple == null || magicHand == null)
            return;

        if (rippleSequence != null)
            rippleSequence.Kill();

        tapRipple.gameObject.SetActive(true);
        tapRipple.rectTransform.position = magicHand.position;
        tapRipple.rectTransform.localScale = Vector3.one * 0.4f;

        Color c = tapRipple.color;
        c.a = 0.6f;
        tapRipple.color = c;

        rippleSequence = DOTween.Sequence();
        rippleSequence.Append(tapRipple.rectTransform.DOScale(1.4f, 0.35f));
        rippleSequence.Join(tapRipple.DOFade(0f, 0.35f));
        rippleSequence.OnComplete(() => tapRipple.gameObject.SetActive(false));
    }

    private Vector2 GetTargetLocalPoint(RectTransform target, RectTransform pointerParent, TutorialTargetPoint point)
    {
        if (target == null || pointerParent == null)
            return Vector2.zero;

        Vector3 worldPoint = GetTargetWorldPoint(target, point);
        return WorldToLocalPoint(worldPoint, target, pointerParent);
    }

    private Vector2 GetArrowLocalPoint(RectTransform target, RectTransform pointerParent, TutorialArrowSide side, float distance)
    {
        TutorialTargetPoint edgePoint = TutorialTargetPoint.Center;

        switch (side)
        {
            case TutorialArrowSide.Above:
                edgePoint = TutorialTargetPoint.Top;
                break;
            case TutorialArrowSide.Below:
                edgePoint = TutorialTargetPoint.Bottom;
                break;
            case TutorialArrowSide.Left:
                edgePoint = TutorialTargetPoint.Left;
                break;
            case TutorialArrowSide.Right:
                edgePoint = TutorialTargetPoint.Right;
                break;
        }

        Vector2 localPoint = GetTargetLocalPoint(target, pointerParent, edgePoint);
        localPoint += GetArrowSideOffset(side, distance);
        return localPoint;
    }

    private Vector3 GetTargetWorldPoint(RectTransform target, TutorialTargetPoint point)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];
        Vector3 topLeft = corners[1];
        Vector3 topRight = corners[2];
        Vector3 bottomRight = corners[3];

        switch (point)
        {
            case TutorialTargetPoint.Top:
                return (topLeft + topRight) * 0.5f;
            case TutorialTargetPoint.Bottom:
                return (bottomLeft + bottomRight) * 0.5f;
            case TutorialTargetPoint.Left:
                return (bottomLeft + topLeft) * 0.5f;
            case TutorialTargetPoint.Right:
                return (bottomRight + topRight) * 0.5f;
            case TutorialTargetPoint.TopLeft:
                return topLeft;
            case TutorialTargetPoint.TopRight:
                return topRight;
            case TutorialTargetPoint.BottomLeft:
                return bottomLeft;
            case TutorialTargetPoint.BottomRight:
                return bottomRight;
            default:
                return (bottomLeft + topRight) * 0.5f;
        }
    }

    private Vector2 WorldToLocalPoint(Vector3 worldPoint, RectTransform target, RectTransform pointerParent)
    {
        Camera targetCamera = GetCanvasCamera(target);
        Camera pointerCamera = GetCanvasCamera(pointerParent);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, worldPoint);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pointerParent,
            screenPoint,
            pointerCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private Camera GetCanvasCamera(Component component)
    {
        Canvas canvas = component.GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (canvas.worldCamera != null)
            return canvas.worldCamera;

        return Camera.main;
    }

    private Vector2 GetArrowSideOffset(TutorialArrowSide side, float distance)
    {
        switch (side)
        {
            case TutorialArrowSide.Above:
                return new Vector2(0f, distance);
            case TutorialArrowSide.Below:
                return new Vector2(0f, -distance);
            case TutorialArrowSide.Left:
                return new Vector2(-distance, 0f);
            case TutorialArrowSide.Right:
                return new Vector2(distance, 0f);
            default:
                return Vector2.zero;
        }
    }

    private Vector2 GetArrowBounceOffset(TutorialArrowSide side, float distance)
    {
        switch (side)
        {
            case TutorialArrowSide.Above:
                return new Vector2(0f, -distance);
            case TutorialArrowSide.Below:
                return new Vector2(0f, distance);
            case TutorialArrowSide.Left:
                return new Vector2(distance, 0f);
            case TutorialArrowSide.Right:
                return new Vector2(-distance, 0f);
            default:
                return Vector2.zero;
        }
    }

    private float GetArrowRotationZ(TutorialArrowSide side)
    {
        // Default assumes your arrow sprite points down when Z rotation is 0.
        float rotation;

        switch (side)
        {
            case TutorialArrowSide.Above:
                rotation = 0f;
                break;
            case TutorialArrowSide.Below:
                rotation = 180f;
                break;
            case TutorialArrowSide.Left:
                rotation = -90f;
                break;
            case TutorialArrowSide.Right:
                rotation = 90f;
                break;
            default:
                rotation = 0f;
                break;
        }

        if (!arrowSpritePointsDownAtZero)
            rotation += 180f;

        return rotation;
    }

    private void KillTweens()
    {
        if (handTapSequence != null)
            handTapSequence.Kill();

        if (arrowBounceSequence != null)
            arrowBounceSequence.Kill();

        if (rippleSequence != null)
            rippleSequence.Kill();

        if (magicHand != null)
        {
            magicHand.DOKill();
            magicHand.localScale = originalHandScale;
        }

        if (arrow != null)
        {
            arrow.DOKill();
            arrow.localScale = originalArrowScale;
        }
    }
}
