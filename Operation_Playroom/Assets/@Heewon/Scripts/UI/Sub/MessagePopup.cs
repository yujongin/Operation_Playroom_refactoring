using DG.Tweening;
using TMPro;
using UnityEngine;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    [SerializeField] CanvasGroup messageArea;

    bool isAnimating = false;

    public void SetText(string text)
    {
        messageText.text = text;
    }

    public void Show()
    {
        if (isAnimating) { return; }

        isAnimating = true;

        gameObject.SetActive(true);
        messageArea.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.Append(messageArea.DOFade(1f, 0.3f))
            .Join(messageArea.transform.DOScale(1.1f, 0.2f))
            .Append(messageArea.transform.DOScale(1f, 0.1f))
            .OnComplete(() => isAnimating = false);
        seq.Play();
    }

    public void Close()
    {
        if (isAnimating) { return; }
        isAnimating = true;

        messageArea.alpha = 1f;
        Sequence seq = DOTween.Sequence();

        seq.Append(messageArea.DOFade(0f, 0.3f));
        seq.Append(messageArea.transform.DOScale(1.1f, 0.1f));
        seq.Append(messageArea.transform.DOScale(0.2f, 0.2f));

        seq.Play().OnComplete(() =>
        {
            isAnimating = false;
            Destroy(gameObject);
        });
    }
}
