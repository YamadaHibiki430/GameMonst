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
    ///<�ϐ�>
    ///=========================================================

    [SerializeField]
    private Vector2 hitPosition = Vector2.zero;
    //1f�O�̌������ꏊ
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
    ///<�֐�>
    ///=========================================================

    private void Awake()
    {
        //�r�b�g���Z���g��Ȃ���2DRay�̓q�b�g���Ȃ�
        hitLayerMask = 1 << LayerMask.NameToLayer("Wall");
        ChangeState(StateType.IDOL);
    }

    private void Update()
    {
        playerStateProcessor.Update();
    }

    /// <summary>
    /// �X�e�[�g�̕ύX
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
    /// ���˂���Ƃ��̃p�����[�^��ݒ�
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
    /// �v���C���[�{�[���̈ړ�
    /// </summary>
    /// <param name="speed"></param>
    public void MovePlayerBall()
    {
        //��Q���ɓ��������ꍇ�̏���
        if(hitDistance != float.MaxValue && hitDistance <= Vector2.Distance(prePosition, transform.position))
        {
            Debug.Log("Hit");
            transform.position = hitPosition;
            moveDirection = ReflectionDirectionVector2(prePosition, moveDirection);

            if (!CheckBallHitPosition())
            {
                Debug.Log("�Փ˂Ȃ�");
            }
        }
        nowMoveSpeed = DecelerationSpeed(initialspeed);
        nowMoveSpeed = Utility.TruncateNumbers(nowMoveSpeed, 3);
        var moveVector = Mathf.Max(nowMoveSpeed, 0) * new Vector3(moveDirection.x, moveDirection.y, 0) * Time.deltaTime;
        transform.position += moveVector;
        //���x��0�ȉ��Ȃ�ҋ@��ԂɕύX
        if(Mathf.Max(nowMoveSpeed, 0) <= 0)
        {
            ChangeState(StateType.IDOL);
        }
    }

    /// <summary>
    /// �����ʂ̌v�Z
    /// </summary>
    /// <returns></returns>
    private float DecelerationSpeed(float initialspeed)
    {
        shotProgressTime += Time.deltaTime;
        return initialspeed * Mathf.Exp(-decelerationSpeed * Mathf.Pow(shotProgressTime, 5) / Mathf.Pow(initialspeed, 2));
    }

    /// <summary>
    /// �X�e�[�^�X�̑��x��1f���Ƃ̈ړ��ʂɕϊ�
    /// </summary>
    /// <param name="statusSpeed"></param>
    /// <returns></returns>
    private float StatusSpeedToMoveSpeed(float statusSpeed)
    {
        return 3.7f * Mathf.Sqrt(statusSpeed);
    }

    /// <summary>
    /// ���ˊp�x�����߂�
    /// </summary>
    /// <param name="prePos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector2 ReflectionDirectionVector2(Vector2 prePos, Vector2 direction)
    {
        RaycastHit2D[] raycasts = Physics2D.CircleCastAll(prePos, ballRadius, direction, float.MaxValue, hitLayerMask);
        var settledColliders = Physics2D.OverlapCircleAll(transform.position, ballRadius, hitLayerMask);
        //�p�ɓ���������(��̕ǂɓ���������)�ɕЕ��̕ǂ�D�悵�Ĕ��肷�邽�߂̏���
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
    /// �{�[���������������H
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
            //���������ʒu�Ɠ����ʒu�̕ǂ�F�����Ȃ��悤�ɂ���
            settledCollider = Physics2D.OverlapCircle(transform.position, ballRadius, hitLayerMask);
        }

        foreach (var raycast in raycasts)
        {
            if (raycast && raycast.collider != settledCollider)
            {
                hitDistance = raycast.distance;
                hitPosition = raycast.centroid;
                result = true;
                //�����������Ԃœ��邽�߂����ɓ���������Ԃ�
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
                Debug.Log("�Փ˂Ȃ�");
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
