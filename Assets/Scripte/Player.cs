using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum StateType
    {
        IDOL,
        SHOTSTANDBY,
        MOVE,
        NONE
    }

    ///=========================================================
    ///<変数>
    ///=========================================================

    [SerializeField]
    private Vector2 hitPosition = Vector2.zero;
    //1f前の元いた場所
    [SerializeField]
    private Vector2 prePosition = Vector2.zero;

    [SerializeField]
    private Vector2 moveDirection = Vector2.zero;

    [SerializeField]
    private float decelerationSpeed = 0.33f;

    [SerializeField]
    private float nowMoveSpeed = 0.0f;

    [SerializeField]
    private float ballRadius = 0.3f;

    [SerializeField]
    private float hitDistance = float.MaxValue;

    [SerializeField]
    private float initialspeed = 0.0f;

    private bool isSettled = false;

    private int hitLayerMask = 0;

    private float shotProgressTime = 0.0f;

    private Collider2D settledCollider = null;

    private PlayerStateProcessor playerStateProcessor = new();

    ///=========================================================
    ///<関数>
    ///=========================================================

    private void Awake()
    {
        //ビット演算を使わないと2DRayはヒットしない
        hitLayerMask = 1 << LayerMask.NameToLayer("Wall");
        ChangeState(StateType.IDOL);
    }

    private void Update()
    {
        playerStateProcessor.Update();
    }

    /// <summary>
    /// ステートの変更
    /// </summary>
    /// <param name="type"></param>
    public void ChangeState(StateType type)
    {
        switch (type)
        {
            case StateType.IDOL:
                playerStateProcessor.ChangeState(new Idol(this));
                break;
            case StateType.MOVE:
                playerStateProcessor.ChangeState(new Move(this));
                break;
        }
    }

    public StateType GetNowStateType()
    {
        return playerStateProcessor.GetNowStateType();
    }

    /// <summary>
    /// 発射するときのパラメータを設定
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void SetShotParameter(Vector2 direction, float speed)
    {
        moveDirection = direction;
        initialspeed = StatusSpeedToMoveSpeed(speed);
        shotProgressTime = 0;
    }

    /// <summary>
    /// プレイヤーボールの移動
    /// </summary>
    /// <param name="speed"></param>
    public void MovePlayerBall()
    {
        //障害物に当たった場合の処理
        if(hitDistance != float.MaxValue && hitDistance <= Vector2.Distance(prePosition, transform.position))
        {
            Debug.Log("Hit");
            transform.position = hitPosition;
            moveDirection = ReflectionDirectionVector2(prePosition, moveDirection);

            if (!CheckBallHitPosition())
            {
                Debug.Log("衝突なし");
            }
        }
        nowMoveSpeed = DecelerationSpeed(initialspeed);
        nowMoveSpeed = Utility.TruncateNumbers(nowMoveSpeed, 3);
        var moveVector = Mathf.Max(nowMoveSpeed, 0) * new Vector3(moveDirection.x, moveDirection.y, 0) * Time.deltaTime;
        transform.position += moveVector;
        //速度が0以下なら待機状態に変更
        if(Mathf.Max(nowMoveSpeed, 0) <= 0)
        {
            ChangeState(StateType.IDOL);
        }
    }

    /// <summary>
    /// 減速量の計算
    /// </summary>
    /// <returns></returns>
    private float DecelerationSpeed(float initialspeed)
    {
        shotProgressTime += Time.deltaTime;
        return initialspeed * Mathf.Exp(-decelerationSpeed * Mathf.Pow(shotProgressTime, 5) / Mathf.Pow(initialspeed, 2));
    }

    /// <summary>
    /// ステータスの速度を1fごとの移動量に変換
    /// </summary>
    /// <param name="statusSpeed"></param>
    /// <returns></returns>
    private float StatusSpeedToMoveSpeed(float statusSpeed)
    {
        return 3.7f * Mathf.Sqrt(statusSpeed);
    }

    /// <summary>
    /// 反射角度を求める
    /// </summary>
    /// <param name="prePos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector2 ReflectionDirectionVector2(Vector2 prePos, Vector2 direction)
    {
        RaycastHit2D[] raycasts = Physics2D.CircleCastAll(prePos, ballRadius, direction, float.MaxValue, hitLayerMask);
        var settledColliders = Physics2D.OverlapCircleAll(transform.position, ballRadius, hitLayerMask);
        //角に当たった時(二つの壁に当たった時)に片方の壁を優先して判定するための処理
        if(settledColliders.Length > 1)
        {
            settledCollider = settledColliders[0];
            isSettled = true;
        }
        else
        {
            isSettled = false;
        }
        foreach (var raycast in raycasts)
        {
            if (raycast && raycast.collider != settledCollider)
            {
                return Vector2.Reflect(direction, raycast.normal);
            }
        }

        return Vector2.zero;
    }

    /// <summary>
    /// ボールが当たったか？
    /// </summary>
    /// <param name="prePos"></param>
    /// <param name="nowPos"></param>
    /// <returns></returns>
    public bool CheckBallHitPosition()
    {
        bool result = false;
        prePosition = transform.position;
        RaycastHit2D[] raycasts = Physics2D.CircleCastAll(transform.position, ballRadius, moveDirection,float.MaxValue, hitLayerMask);

        if (!isSettled)
        {
            //当たった位置と同じ位置の壁を認識しないようにする
            settledCollider = Physics2D.OverlapCircle(transform.position, ballRadius, hitLayerMask);
        }

        foreach (var raycast in raycasts)
        {
            if (raycast && raycast.collider != settledCollider)
            {
                hitDistance = raycast.distance;
                hitPosition = raycast.centroid;
                result = true;
                //当たった順番で入るためすぐに当たったら返す
                return result;
            }
        }

        return result;
    }

    public abstract class PlayerStateBase : StateBase
    {
        protected Player player = null;

        public PlayerStateBase(Player player)
        {
            this.player = player;
        }

        public virtual StateType GetStateType
        {
            get
            {
                return StateType.NONE;
            }
        }
    }

    public class Idol : PlayerStateBase
    {
        public Idol(Player player) : base(player)
        {
            this.player = player;
        }

        public override StateType GetStateType
        {
            get
            {
                return StateType.IDOL;
            }
        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }

    public class ShotStandby : PlayerStateBase
    {
        public ShotStandby(Player player) : base(player)
        {
            this.player = player;
        }

        public override StateType GetStateType
        {
            get
            {
                return StateType.SHOTSTANDBY;
            }
        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }

    public class Move : PlayerStateBase
    {
        private bool isHit = false;

        public Move(Player player) : base(player)
        {
            this.player = player;
        }

        public override StateType GetStateType
        {
            get
            {
                return StateType.MOVE;
            }
        }

        public override void OnEnter()
        {
            if (!player.CheckBallHitPosition())
            {
                Debug.Log("衝突なし");
            }
            else
            {
                isHit = true;
            }
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            if (isHit)
            {
                player.MovePlayerBall();
            }
        }
    }

    public class PlayerStateProcessor : StateProcessorBase
    {
        public StateType GetNowStateType()
        {
            PlayerStateBase playerState = (PlayerStateBase)stateBase;
            return playerState.GetStateType;
        }
    }
}
