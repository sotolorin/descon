using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;

[RequireComponent(typeof(ShapeControl))]
[RequireComponent(typeof(LineDrawing))]
public class MemberControl2 : MonoBehaviour
{
    private struct DblVector4
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public DblVector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static bool operator ==(DblVector4 a, DblVector4 b)
        {
            if (a.x.Equals(b.x) && a.y.Equals(b.y) && a.z.Equals(b.z) && a.w.Equals(b.w))
                return true;
            else
                return false;
        }

        public static bool operator !=(DblVector4 a, DblVector4 b)
        {
            if (!a.x.Equals(b.x) || !a.y.Equals(b.y) || !a.z.Equals(b.z) || !a.w.Equals(b.w))
                return true;
            else
                return false;
        }
    }

    public ShapeControl shapeControl;
    public LineDrawing lineDrawing;
    public EMemberType memberType;
    public EMemberSubType subType;
    public bool isSelected = false;
    public Color memberColor;
    private float highlightStrength = 0.1f;

    private List<MemberControl2> connectionMembers;

    //These members are used to see if the shape changed
    private DblVector4 prevMemberSize;
    private DblVector4 prevConnectionSize;
    private EShearCarriedBy prevShearConnection;
    private EMomentCarriedBy prevMomentConnection;
    private ESeatStiffener prevSeatStiffener;
    private double prevYOffsetAndEl;

    /// <summary>
    /// Warning, once called, overwrites the prev size 
    /// </summary>
    public bool ShearConnChanged
    {
        get
        {
            var detail = CommonDataStatic.DetailDataDict[memberType];
            var elOffset = DrawingMethods.GetYOffsetWithEl(detail);

            if(detail != null)
            {
                //Check shear change
                if(prevShearConnection != detail.ShearConnection)
                {
                    prevShearConnection = detail.ShearConnection;
                    return true;
                }
                else if (prevYOffsetAndEl != elOffset)
                {
                    prevYOffsetAndEl = elOffset;
                    return true;
                }
                else
                {
                    DblVector4 size = new DblVector4(-1, -1, -1, -1);
                    switch(detail.ShearConnection)
                    {
                        case EShearCarriedBy.ClipAngle:
                            var angle = detail.WinConnect.ShearClipAngle;
                            size = new DblVector4(angle.ShortLeg, angle.LongLeg, angle.Length, angle.Thickness);

                            if(size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            break;

                        case EShearCarriedBy.EndPlate:
                            var endplate = detail.WinConnect.ShearEndPlate;
                            size = new DblVector4(endplate.Width, endplate.Length, endplate.Thickness, 0);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            break;

                        case EShearCarriedBy.Seat:
                            var seat = detail.WinConnect.ShearSeat;
                            size = new DblVector4(seat.TopAngleSupLeg, seat.TopAngleBeamLeg, seat.TopAngleLength, seat.TopAngle.t);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            //Bottom angle/stiffener
                            if (prevSeatStiffener != seat.Stiffener)
                            {
                                prevSeatStiffener = seat.Stiffener;
                                return true;
                            }
                            else
                            {
                                //TODO fill this out
                                switch (seat.Stiffener)
                                {
                                    case ESeatStiffener.None:
                                        break;

                                    case ESeatStiffener.L2:
                                        break;

                                    case ESeatStiffener.Plate:
                                        break;

                                    case ESeatStiffener.Tee:
                                        break;
                                }
                            }

                            break;

                        case EShearCarriedBy.SinglePlate:
                            var singleplate = detail.WinConnect.ShearWebPlate;
                            size = new DblVector4(singleplate.Width, singleplate.Length, singleplate.Thickness, 0);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }
                            break;

                        case EShearCarriedBy.Tee:
                            var tee = detail.WinConnect.ShearWebTee;
                            size = new DblVector4(tee.Size.bf, tee.Size.d, tee.Size.tw, tee.SLength);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }
                            break;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Warning, once called, overwrites the prev size 
    /// </summary>
    public bool MomentConnChanged
    {
        get
        {
            var detail = CommonDataStatic.DetailDataDict[memberType];
            var elOffset = DrawingMethods.GetYOffsetWithEl(detail);

            if (detail != null)
            {
                //Check shear change
                if (prevMomentConnection != detail.MomentConnection)
                {
                    prevMomentConnection = detail.MomentConnection;
                    return true;
                }
                else if (prevYOffsetAndEl != elOffset)
                {
                    prevYOffsetAndEl = elOffset;
                    return true;
                }
                else
                {
                    DblVector4 size = new DblVector4(-1, -1, -1, -1);
                    switch (detail.MomentConnection)
                    {
                        case EMomentCarriedBy.NoMoment:
                            break;

                        case EMomentCarriedBy.Angles:
                            var angle = detail.WinConnect.MomentFlangeAngle;
                            size = new DblVector4(angle.Angle.b, angle.Angle.d, angle.Angle.t, angle.Length);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            break;

                        case EMomentCarriedBy.DirectlyWelded:
                            break;

                        case EMomentCarriedBy.FlangePlate:
                            var plate = detail.WinConnect.MomentFlangePlate;
                            size = new DblVector4(plate.TopLength, plate.TopThickness, plate.TopWidth, 0);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            break;

                        case EMomentCarriedBy.Tee:
                            var tee = detail.WinConnect.MomentTee;
                            size = new DblVector4(tee.TopTeeShape.bf, tee.TopTeeShape.d, tee.TopTeeShape.tw, tee.TopLengthAtStem);

                            if (size != prevConnectionSize)
                            {
                                prevConnectionSize = size;
                                return true;
                            }

                            break;
                    }
                }
            }

            return false;
        }
    }

    public bool MemberChanged
    {
        get
        {
            var detail = CommonDataStatic.DetailDataDict[memberType];
            DblVector4 size = new DblVector4(detail.Shape.USShape.d, detail.Shape.USShape.bf, detail.Shape.USShape.Ht, detail.Shape.USShape.B);

            if (size != prevMemberSize)
            {
                prevMemberSize = size;
                return true;
            }

            return false;
        }
    }

    void Awake()
    {
        connectionMembers = new List<MemberControl2>();
    }

    public List<MemberControl2> GetConnectionMembers()
    {
        return connectionMembers;
    }

    public void AddConnectionMember(MemberControl2 connection)
    {
        connectionMembers.Add(connection);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);

        //Disable the drawing lines
        lineDrawing.SetVisible(visible);

        //Disable the connections
        foreach(var conn in connectionMembers)
        {
            conn.SetVisible(visible);
        }
    }

    public void DestroyConnectionMeshes()
    {
        foreach (var item in connectionMembers)
        {
            DestroyImmediate(item.gameObject);
        }

        connectionMembers.Clear();
    }

    public void SetSelected(bool selected, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        isSelected = selected;

        if (isSelected)
        {
            var c = MessageQueueTest.GetSelectedColor();
            shapeControl.Color = c;
            lineDrawing.Color = c;

            if (sendMessage)
            {
                var click = Descon.Data.EClickType.Single;

                if (doubleClicked)
                    click = Descon.Data.EClickType.Double;
                else if (mouseButton == 1)
                {
                    click = Descon.Data.EClickType.Right;
                }

                MessageQueueTest.instance.SendUnityData(MessageQueueTest.GetClickString(memberType, subType, click));
            }
        }
        else
        {
            if (isSelected)
            {
                var c = MessageQueueTest.GetSelectedColor();
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
            else
            {
                var c = memberColor;
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
        }
    }

    public void SetSelected(GameObject obj, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        SetSelected(obj == gameObject, sendMessage, mouseButton, doubleClicked);
    }

    public void SetHover(GameObject obj)
    {
        if (obj == gameObject)
        {
            if (isSelected)
            {
                var c = MessageQueueTest.GetSelectedColor();
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
            else
            {
                var c = memberColor + new Color(highlightStrength, highlightStrength, highlightStrength, 1);
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
        }
        else
        {
            if (isSelected)
            {
                var c = MessageQueueTest.GetSelectedColor();
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
            else
            {
                //Set to normal colors
                var c = memberColor;
                shapeControl.Color = c;
                lineDrawing.Color = c;
            }
        }
    }
}