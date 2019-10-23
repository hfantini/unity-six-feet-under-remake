﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Tile
{
    // == VAR & CONST ========================================================================================================

    protected const int ANIM_DELAY = 70;
    protected const int MOVEMENT_DELAY = 150;
    protected const int HEAVY_MOVE_EFFORT = 3;
    protected float _lastAnim = 0;
    protected float _lastMovement = 0;
    protected int _moveHeavyEffortCount = 0;
    protected int _currentAnimSeq = 0;
    protected Sprite[] _animSpriteSeq;

    // == METHODS ============================================================================================================

    public Player( ScriptGame parent, Vector2 initialPos) : base(parent, initialPos)
    {
        this._type = TileType.TYPE_DYNAMIC;
        this._faceDirection = TileFaceDirection.RIGHT;

        this._currentAnimSeq = 0;
        this._animSpriteSeq = new Sprite[]
        {
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK01" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK02" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK03" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK04" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK05" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK06" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK07" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK08" ),
            this._sprite = Resources.Load<Sprite>( "Sprites/WALK09" ),
        };

        this._sprite = this._animSpriteSeq[this._currentAnimSeq];
    }

    protected void updateAnimation()
    {
        if ((Time.time * 1000) - this._lastAnim > ANIM_DELAY)
        {
            this._currentAnimSeq++;

            if (this._currentAnimSeq > this._animSpriteSeq.Length - 1)
            {
                this._currentAnimSeq = 0;
            }

            this._sprite = this._animSpriteSeq[this._currentAnimSeq];
            this._lastAnim = Time.time * 1000;
        }
    }

    public override void update()
    {
        base.update();
        bool move = false;
        Vector2 nextPos = Vector2.zero;
        TileMovementDirection currentMovement = TileMovementDirection.NO_MOVEMENT;

        // CHECK MOVEMENT

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if ((Time.time * 1000) - this._lastMovement > MOVEMENT_DELAY)
            {
                move = true;
                nextPos = this._position + new Vector2(-1, 0);
                _lastMovement = Time.time * 1000;
                this._faceDirection = TileFaceDirection.LEFT;
                currentMovement = TileMovementDirection.LEFT;
            }

            this.updateAnimation();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if ((Time.time * 1000) - this._lastMovement > MOVEMENT_DELAY)
            {
                move = true;
                nextPos = this._position + new Vector2(1, 0);
                _lastMovement = Time.time * 1000;
                this._faceDirection = TileFaceDirection.RIGHT;
                currentMovement = TileMovementDirection.RIGHT;
            }

            this.updateAnimation();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if ((Time.time * 1000) - this._lastMovement > MOVEMENT_DELAY)
            {
                move = true;
                nextPos = this._position + new Vector2(0, -1);
                _lastMovement = Time.time * 1000;
                currentMovement = TileMovementDirection.UP;
            }

            this.updateAnimation();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if ((Time.time * 1000) - this._lastMovement > MOVEMENT_DELAY)
            {
                move = true;
                nextPos = this._position + new Vector2(0, 1);
                _lastMovement = Time.time * 1000;
                currentMovement = TileMovementDirection.DOWN;
            }

            this.updateAnimation();
        }

        // UPDATE MOVEMENT

        if( move )
        {
            Tile colisionTile = this._parent.getTileOnPosition( nextPos );

            if ( !( colisionTile is Wall ) )
            {
                if (colisionTile is Exit)
                {
                    if (((Exit)colisionTile).open)
                    {
                        this._parent.setTilePosition(this, nextPos);

                        // LEVEL FINISHED
                        this._parent.completeLevel();
                    }

                    this._moveHeavyEffortCount = 0;
                }
                else if (colisionTile is Collectible)
                {
                    if (colisionTile is Gem)
                    {
                        SessionData.score += ((Gem)colisionTile).score;
                        this._parent.collectGem();
                        this._parent.setTilePosition(this, nextPos);
                    }
                    else if (colisionTile is Score)
                    {
                        SessionData.score += ((Score)colisionTile).score;
                        this._parent.setTilePosition(this, nextPos);
                    }
                    else if( colisionTile is Clock )
                    {
                        this._parent.collectClock();
                        this._parent.setTilePosition(this, nextPos);
                    }
                    else if (colisionTile is Power)
                    {
                        SessionData.lives++;
                        this._parent.setTilePosition(this, nextPos);
                    }

                    this._parent.deleteDynamicTile(colisionTile);

                    this._moveHeavyEffortCount = 0;
                }
                else if (colisionTile is Heavy)
                {
                    if(this._moveHeavyEffortCount > HEAVY_MOVE_EFFORT)
                    {
                        // MOVE THE HEAVY OBJECT
                        if (currentMovement == TileMovementDirection.RIGHT)
                        {
                            Tile moveSpace = this._parent.getTileOnPosition(colisionTile.position + new Vector2(1, 0) );

                            if( moveSpace == null || moveSpace is Empty)
                            {
                                this._parent.setTilePosition(colisionTile, colisionTile.position + new Vector2(1, 0) );
                                this._parent.setTilePosition(this, this.position + new Vector2(1, 0) );
                            }
                        }
                        else if (currentMovement == TileMovementDirection.LEFT)
                        {
                            Tile moveSpace = this._parent.getTileOnPosition(colisionTile.position + new Vector2(-1, 0));

                            if (moveSpace == null || moveSpace is Empty)
                            {
                                this._parent.setTilePosition(colisionTile, colisionTile.position + new Vector2(-1, 0));
                                this._parent.setTilePosition(this, this.position + new Vector2(-1, 0));
                            }
                        }

                        // RESET THE COUNTER 
                        this._moveHeavyEffortCount = 0;
                    }

                    this._moveHeavyEffortCount++;
                }
                else
                {
                    this._moveHeavyEffortCount = 0;
                    this._parent.setTilePosition(this, nextPos);
                }
            }
        }
    }

    // == EVENTS =============================================================================================================
}
