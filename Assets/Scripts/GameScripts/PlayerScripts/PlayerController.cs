using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

    private readonly WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    private readonly WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    

    [Header("References"), Space]
    [SerializeField]
    private GridMatrix gridMatrixSO;

    [SerializeField] private GameStateData stateData;
    [SerializeField] private GameParseData gameParseData;


    [Header("Movement Variables"), Space]
    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator magicianAnimator;


    private List<DirectionChange> wayTofollow = new List<DirectionChange>();
    private int goblinCellValue;
    private Vector2Int lastGridPos;
    private int currentLevel;

    public Action OnGoblinDead = delegate { };
    public Action OnShake = delegate { };
    public Action OnGoblinGetTresaure = delegate { };
    public Action<bool> showTrapsAround = delegate { };
    public Action<int> onCellScore = delegate { };

    private void OnEnable()
    {
        // escuchamos cuando se crea un camino
        stateData.OnUpdatePath += MoveGoblin;
        // el personaje inicia con opacidad 0
        playerSprite.color = new Color(1f, 1f, 1f, 0f);
        OnCreate();
    }

    private void OnDisable()
    {
        stateData.OnUpdatePath -= MoveGoblin;
    }

    private void OnCreate()
    {
        currentLevel = stateData.currentStateData.level;

        Sequence spawnGoblin = DOTween.Sequence();
        spawnGoblin.Append(playerSprite.DOFade(1f, 0.45f).SetEase(Ease.Linear));
        spawnGoblin.Join(playerSprite.transform.DOLocalMove(new Vector3(0, 0.25f, 0), 0.5f).SetEase(Ease.Linear));
        //spawnGoblin.Append(playerSprite.transform.DOPunchScale(new Vector3(0f, 0.5f, 0f), 0.25f, 1, 0.5f)
        //    .SetEase(Ease.Linear));
    }

    private void MoveGoblin(List<MovedStruct> path)
    {
        if (path == null || path.Count == 0) return;

        // asignamos los valores de puntuación a las celdas del camino
        SetValuesToPathCells(path);

        // Extraer solo las coordenadas
        List<Vector2Int> pathClone = path.Select(p => p.Coordinate).ToList();


        lastGridPos = pathClone.Last();
        goblinCellValue = gameParseData.GetCellValue(lastGridPos.x, lastGridPos.y);

        Vector2Int? goblinIndexPosition = FindObjectOfType<PathBuilder>().GetPlayerStartPosition();
        if (goblinIndexPosition.HasValue)
        {
            pathClone.Insert(0, goblinIndexPosition.Value);
            Debug.Log($"Agregada posición inicial del goblin: {goblinIndexPosition.Value}");
        }

        wayTofollow = MovementPlayer.DetectDirectionChanges(pathClone, null);

        if (wayTofollow != null && wayTofollow.Count > 0)
        {
            foreach (DirectionChange change in wayTofollow)
            {
                Debug.Log($"Pos: {change.Position} - Dir: {change.NewDirection}");
            }

            FollowPath();
        }
    }


    private bool GotTreasure(int value) => value > currentLevel;
    private bool IsPlaceDead(int value) => value >= 0 && value < 9;


    private void FollowPath()
    {
        if (wayTofollow == null || wayTofollow.Count == 0)
        {
            Debug.LogWarning("No hay puntos de dirección para seguir.");
            return;
        }

        Sequence moveSequence = DOTween.Sequence();
        moveSequence.SetId(GoblinAnimations.GoblinMove.ToString());
        Vector3 currentPos = transform.position;

        foreach (DirectionChange change in wayTofollow)
        {
            Vector3 worldTarget = GridToWorldPosition(change.Position);

            float distance = Vector2.Distance(currentPos, worldTarget);
            float logValue = Mathf.Log(distance + 1f);
            float safeDenominator = Mathf.Max(0.01f, moveSpeed * logValue);
            float duration = distance / safeDenominator;

            Vector2Int dir = change.NewDirection;
            moveSequence.Append(transform.DOMove(worldTarget, duration).SetEase(Ease.InSine));
            moveSequence.AppendCallback(() =>
            {
                // arreglamos la orientación del personaje en movimiento vertical
                Vector2Int fixYDir = new Vector2Int(1, -1);
                // animacion previa al movimiento
                SetAnimationByDirection(dir * fixYDir);
            });

            currentPos = worldTarget;
        }

        moveSequence.OnComplete(() =>
        {
            int newLevel = stateData.currentStateData.level;

            if (IsPlaceDead(goblinCellValue))
            {
                // activamos animación de la trampa
                Transform tile = gridMatrixSO.GetPosition(lastGridPos.x, lastGridPos.y);
                if (tile != null)
                {
                    int val = gameParseData.GetCellValue(lastGridPos.x, lastGridPos.y);
                    // string strVal = val == 0 ? "" : val.ToString();
                    tile.gameObject.GetComponent<TileInfo>().BecameTrap(val.ToString());
                    // activamos un splash de sangre
                    FindObjectOfType<BloodRandomManager>().EnableFirstInactiveChild(tile.transform.position);
                }

                // DeadGoblin();
                transform.GetChild(0).gameObject.SetActive(false);
                // transform.GetChild(1).gameObject.SetActive(true);

                StartCoroutine(DelaySpawnGoblin());
            }
            else if (GotTreasure(newLevel))
            {
                // activamos la animación del mago con el tesoro
                // GotTreasure();
                // llamar a la animación de mago con el tesoro
                // al finalizar la animación se resetea la grilla
                if (stateData.currentStateData.gameOver) return;
                Debug.Log("Tesoro Conseguido");
                currentLevel = newLevel;

                // detenemos la animación del goblin
                animator.Play("playerIdle");

                // activamos el animator del mago
                Transform handMage = magicianAnimator.transform.GetChild(0);
                handMage.gameObject.SetActive(true);
                Transform chest = GameObject.FindWithTag("Chest").transform;
                Vector2 handPosition = chest.position;
                handMage.transform.position = new Vector3(handPosition.x, handPosition.y, 0);
                magicianAnimator.CrossFade("Mago_Tresaure", 0.1f);
                AudioController.Instance.PlaySound(AudioIds.smileGoblin.ToString());


                StartCoroutine(DelaySpawmMagician(chest.gameObject));
            }
            else
            {
                animator.Play("playerIdle");
            }

            FindObjectOfType<PathBuilder>().ResetPath();
            AudioController.Instance.ResetPitchIncremental();
            Debug.Log("Se ha completado el movimiento.");
        });
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        Transform tileTransform = gridMatrixSO.GetPosition(gridPos.x, gridPos.y);
        return tileTransform != null ? tileTransform.position : transform.position;
    }

    private void SetAnimationByDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
        {
            print($"playerWalkUp: {direction}");
            animator.CrossFade("playerWalkUp", 0.1f);
        }
        else if (direction == Vector2Int.down)
        {
            print($"playerWalkDown: {direction}");
            animator.CrossFade("playerWalkDown", 0.1f);
        }
        else if (direction == Vector2Int.left)
        {
            print($"playerWalkLeft: {direction}");
            animator.CrossFade("playerWalk", 0.1f);
            playerSprite.flipX = false;
        }
        else if (direction == Vector2Int.right)
        {
            print($"playerWalkRight: {direction}");
            animator.CrossFade("playerWalk", 0.1f);
            playerSprite.flipX = true;
        }
        else
        {
            Debug.LogWarning($"Dirección inesperada: {direction}");
        }
    }

    // asigna el valor de la puntuación a las celdas del camino
    private void SetValuesToPathCells(List<MovedStruct> path)
    {
        foreach (MovedStruct move in path)
        {
            Vector2Int pos = move.Coordinate;
            int score = move.Score;

            Transform tileTransform = gridMatrixSO.GetPosition(pos.x, pos.y);
            if (tileTransform != null)
            {
                TileInfo tileInfo = tileTransform.GetComponent<TileInfo>();
                if (tileInfo != null)
                {
                    tileInfo.scoreToGive = score;
                }
            }
        }
    }

    private IEnumerator DelaySpawnGoblin()
    {
        AudioController.Instance.PlaySound(AudioIds.deadGoblin.ToString());
        AudioController.Instance.PlaySound(AudioIds.arrowTrap.ToString());
        magicianAnimator.CrossFade("MagoGoblinDeat", 0.1f);
        OnShake?.Invoke();

        yield return new WaitForSeconds(0.25f);

        showTrapsAround?.Invoke(false);
        OnGoblinDead?.Invoke();
        AudioController.Instance.PlaySound(AudioIds.smileGoblin.ToString());
    }

    private IEnumerator DelaySpawmMagician(GameObject chestObj)
    {
        transform.GetChild(0).gameObject.SetActive(false);
        chestObj.SetActive(false);
        yield return _waitForSeconds0_1;
        AudioController.Instance.PlaySound(AudioIds.smileMage.ToString());

        yield return _waitForSeconds1;
        OnGoblinGetTresaure?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Cell"))
        {
            TileInfo tileInfo = collision.GetComponent<TileInfo>();
            if (tileInfo != null && tileInfo.scoreToGive > 0)
            {
                Transform textPoint = tileInfo.transform.GetChild(2).GetChild(0);
                tileInfo.transform.GetChild(3).gameObject.SetActive(true); // habiliatamos la celda verde(visual)
                MeshRenderer meshRenderer = textPoint.GetComponent<MeshRenderer>();
                meshRenderer.sortingLayerName = "MaskLigth";
                meshRenderer.sortingOrder = 6;
                onCellScore?.Invoke(tileInfo.scoreToGive);
                textPoint.GetComponent<TextMeshPro>().text = tileInfo.scoreToGive.ToString();
                tileInfo.transform.GetChild(2).gameObject.SetActive(true);
                AudioController.Instance.PlaySoundIncremental(AudioIds.pointCell.ToString());
                Debug.Log($"Cell: {tileInfo.indice} - Score: {tileInfo.scoreToGive}");
            }
        }
    }
}