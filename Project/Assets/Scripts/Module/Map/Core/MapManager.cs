﻿using Common;
using MVC;
using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RPG GAME/Manager/地图编辑器")]
    public class MapManager : SingletonMonoBehaviour<MapManager>
    {
        /// <summary>
        /// 所控制的地图UI
        /// </summary>
        [SerializeField]
        private MapUI UI;
        /// <summary>
        /// 地图遮罩UI位置
        /// </summary>
        public RectTransform MapMaskRect => UI ? UI.mapMaskRect : null;
        /// <summary>
        /// 地图刷新模式
        /// </summary>
        [SerializeField]
        private UpdateMode updateMode;
        /// <summary>
        /// 玩家
        /// </summary>
        [SerializeField]
        private Transform player;
        /// <summary>
        /// 偏移量
        /// </summary>
        [SerializeField]
        private Vector2 offset;
        /// <summary>
        /// 玩家图标
        /// </summary>
        [SerializeField]
        private Sprite playerIcon;
        /// <summary>
        /// 玩家图标大小
        /// </summary>
        [SerializeField]
        private Vector2 playerIconSize = new Vector2(64, 64);
        /// <summary>
        /// 所产生的玩家图标实例
        /// </summary>
        private MapIcon playerIconInstance;
        /// <summary>
        /// 默认的标志图像
        /// </summary>
        [SerializeField]
        private Sprite defaultMarkIcon;
        /// <summary>
        /// 默认的标志大小
        /// </summary>
        [SerializeField]
        private Vector2 defaultMarkSize = new Vector2(64, 64);
        /// <summary>
        /// 地图相机预制体
        /// </summary>
        [SerializeField]
        private MapCamera cameraPrefab;
        /// <summary>
        /// 地图相机
        /// </summary>
        [SerializeField]
        private MapCamera mapCamera;

        private CanvasGroup gameCanvas;
        public Camera MapCamera
        {
            get
            {
                if (!mapCamera) mapCamera = Instantiate(cameraPrefab, transform);
                return mapCamera.Camera;
            }
        }
        /// <summary>
        /// 目标渲染图像
        /// </summary>
        [SerializeField]
        private RenderTexture targetTexture;
        /// <summary>
        /// 渲染大小
        /// </summary>
        [SerializeField]
        private Vector2Int textureSize = new Vector2Int(1024, 1024);
        /// <summary>
        /// 渲染格式
        /// </summary>
        [SerializeField]
        private RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32;
        /// <summary>
        /// 渲染层级
        /// </summary>
        [SerializeField]
        private LayerMask mapRenderMask = ~0;
        /// <summary>
        /// 是否应用2D
        /// </summary>
        [SerializeField]
        private bool use2D = true;
        /// <summary>
        /// 是否旋转地图
        /// </summary>
        [SerializeField, Tooltip("否则旋转图标。")]
        private bool rotateMap;
        /// <summary>
        /// 是否是圆形区域
        /// </summary>
        [SerializeField]
        private bool circle;
        /// <summary>
        /// 边缘大小
        /// </summary>
        [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
        private float edgeSize;
        /// <summary>
        /// 半径
        /// </summary>
        [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0.5f, 1)]
        private float radius = 1;
        /// <summary>
        /// 大地图边缘大小
        /// </summary>
        [SerializeField, Tooltip("此值为地图遮罩Rect宽度、高度两者中较小值的倍数。"), Range(0, 0.5f)]
        private float worldEdgeSize;
        /// <summary>
        /// 是否处于大地图模式
        /// </summary>
        [SerializeField]
        private bool isViewingWorldMap;
        public bool IsViewingWorldMap => isViewingWorldMap;
        /// <summary>
        /// 拖拽灵敏度
        /// </summary>
        [SerializeField, Range(0.05f, 0.5f)]
        private float dragSensitivity = 0.135f;
        /// <summary>
        /// 动画速度
        /// </summary>
        [SerializeField, Tooltip("小于等于 0 时表示不动画。")]
        private float animationSpeed = 5;
        /// <summary>
        /// 是否可播放动画
        /// </summary>
        private bool AnimateAble => animationSpeed > 0 && miniModeInfo.mapAnchoreMax == worldModeInfo.mapAnchoreMax && miniModeInfo.mapAnchoreMin == worldModeInfo.mapAnchoreMin
            && miniModeInfo.windowAnchoreMax == worldModeInfo.windowAnchoreMax && miniModeInfo.windowAnchoreMin == worldModeInfo.windowAnchoreMin;
        /// <summary>
        /// 是否正在切换
        /// </summary>
        private bool isSwitching;
        /// <summary>
        /// 切换时长
        /// </summary>
        private float switchTime;
        private float startSizeOfCamForMap;
        private Vector3 startPosOfCamForMap;
        private Vector2 startPositionOfMap;
        private Vector2 startSizeOfMapWindow;
        private Vector2 startSizeOfMap;
        /// <summary>
        /// 是否正在移动相机
        /// </summary>
        private bool isMovingCamera;
        private float cameraMovingTime;
        private Vector3 camMoveDestination;

        private Vector2 zoomLimit;

        [SerializeField]
        private MapModeInfo miniModeInfo = new MapModeInfo();

        [SerializeField]
        private MapModeInfo worldModeInfo = new MapModeInfo();
        /// <summary>
        /// 图标生成器与图标对应集合
        /// </summary>
        private readonly Dictionary<MapIconHolder, MapIcon> iconsWithHolder = new Dictionary<MapIconHolder, MapIcon>();
        /// <summary>
        /// 图标与图标生成器对应集合
        /// </summary>
        public Dictionary<MapIcon, MapIconWithoutHolder> IconsWithoutHolder { get; private set; } = new Dictionary<MapIcon, MapIconWithoutHolder>();
        #region 地图图标相关
        /// <summary>
        /// 产生图标
        /// </summary>
        /// <param name="holder">图标生成器</param>
        public void CreateMapIcon(MapIconHolder holder)
        {
            if (!UI || !UI.gameObject || !holder.icon) return;
            MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(holder.iconType)).GetComponent<MapIcon>();
            InitIcon();
            iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
            if (iconFound != null)
            {
                holder.iconInstance = icon;
                iconsWithHolder[holder] = icon;
            }
            else iconsWithHolder.Add(holder, icon);
            return;
            //初始图标
            void InitIcon()
            {
                icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.iconImage.overrideSprite = holder.icon;
                icon.iconImage.rectTransform.sizeDelta = holder.iconSize;
                icon.iconType = holder.iconType;
                icon.RemoveAble = holder.removeAble;
                holder.iconInstance = icon;
                icon.holder = holder;
                if (holder.showRange) icon.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
            }
        }
        /// <summary>
        /// 产生图标（矩形）
        /// </summary>
        /// <param name="iconSprite">图标图像</param>
        /// <param name="size">图标大小</param>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="keepOnMap">是否保持在地图中</param>
        /// <param name="iconType">图标类型</param>
        /// <param name="removeAble">是否可移除</param>
        /// <param name="textToDisplay">显示的文本内容</param>
        /// <returns></returns>
        public MapIcon CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap,
            MapIconType iconType, bool removeAble, string textToDisplay = "")
        {
            if (!UI || !UI.gameObject || !iconSprite) return null;
            MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(iconType)).GetComponent<MapIcon>();
            InitIcon();
            IconsWithoutHolder.Add(icon, new MapIconWithoutHolder(worldPosition, icon, keepOnMap, removeAble, textToDisplay));
            return icon;

            void InitIcon()
            {
                icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.iconImage.overrideSprite = iconSprite;
                icon.iconImage.rectTransform.sizeDelta = size;
                icon.iconType = iconType;
                icon.RemoveAble = removeAble;
                icon.TextToDisplay = textToDisplay;
            }
        }
        /// <summary>
        /// 产生图标（圆形范围)
        /// </summary>
        /// <param name="iconSprite"></param>
        /// <param name="size"></param>
        /// <param name="worldPosition"></param>
        /// <param name="keepOnMap"></param>
        /// <param name="rangeSize"></param>
        /// <param name="iconType"></param>
        /// <param name="removeAble"></param>
        /// <param name="textToDisplay"></param>
        /// <returns></returns>
        public MapIcon CreateMapIcon(Sprite iconSprite, Vector2 size, Vector3 worldPosition, bool keepOnMap, float rangeSize,
            MapIconType iconType, bool removeAble, string textToDisplay = "")
        {
            if (!UI || !UI.gameObject || !iconSprite) return null;
            MapIcon icon = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(iconType)).GetComponent<MapIcon>();
            InitIcon();
            IconsWithoutHolder.Add(icon, new MapIconWithoutHolder(worldPosition, icon, keepOnMap, removeAble, textToDisplay));
            return icon;

            void InitIcon()
            {
                icon.iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.iconImage.overrideSprite = iconSprite;
                icon.iconImage.rectTransform.sizeDelta = size;
                icon.iconType = iconType;
                if (rangeSize > 0)
                {
                    icon.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
                    ZetanUtility.SetActive(icon.iconRange.gameObject, true);
                    icon.iconRange.rectTransform.sizeDelta = new Vector2(rangeSize, rangeSize);
                }
                icon.RemoveAble = removeAble;
                icon.TextToDisplay = textToDisplay;
            }
        }
        /// <summary>
        /// 产生默认图标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="keepOnMap"></param>
        /// <param name="removeAble"></param>
        /// <param name="textToDisplay"></param>
        /// <returns></returns>
        public MapIcon CreateDefaultMark(Vector3 worldPosition, bool keepOnMap, bool removeAble, string textToDisplay = "")
        {
            return CreateMapIcon(defaultMarkIcon, defaultMarkSize, worldPosition, keepOnMap, MapIconType.Mark, removeAble, textToDisplay);
        }
        /// <summary>
        /// 在鼠标点击的位置产生默认图标
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        public MapIcon CreateDefaultMarkAtMousePos(Vector3 mousePosition)
        {
            return CreateDefaultMark(MapPointToWorldPoint(mousePosition), true, true);
        }
        /// <summary>
        /// 移除图标（根据地图图标生成器)
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="force"></param>
        public void RemoveMapIcon(MapIconHolder holder, bool force = false)
        {
            if (!holder || !holder.removeAble && !force) { Debug.Log("return1" + holder); return; }
            //Debug.Log("remove");
            iconsWithHolder.TryGetValue(holder, out MapIcon iconFound);
            if (!iconFound && holder.iconInstance) iconFound = holder.iconInstance;
            if (iconFound) iconFound.Recycle();
            iconsWithHolder.Remove(holder);
        }
        /// <summary>
        /// 移除图标（根据图标信息）
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="force"></param>
        public void RemoveMapIcon(MapIcon icon, bool force = false)
        {
            if (!icon || !icon.RemoveAble && !force) return;
            if (icon.holder) RemoveMapIcon(icon.holder, force);
            else
            {
                IconsWithoutHolder.Remove(icon);
                icon.Recycle();
            }
        }
        /// <summary>
        /// 根据图标所在位置移除图标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="force"></param>
        public void RemoveMapIcon(Vector3 worldPosition, bool force = false)
        {
            foreach (var iconWoH in IconsWithoutHolder.Values.ToList())
                if (iconWoH.worldPosition == worldPosition && (iconWoH.removeAble || force))
                {
                    IconsWithoutHolder.Remove(iconWoH.mapIcon);
                    if (iconWoH.mapIcon) iconWoH.mapIcon.Recycle();
                    iconWoH.mapIcon = null;
                }
        }
        /// <summary>
        /// 销毁图标
        /// </summary>
        /// <param name="icon"></param>
        public void DestroyMapIcon(MapIcon icon)
        {
            if (!icon) return;
            if (icon.holder)
            {
                RemoveMapIcon(icon.holder, true);
            }
            else
            {
                if (!icon.holder) IconsWithoutHolder.Remove(icon);
                else iconsWithHolder.Remove(icon.holder);
            }
            Destroy(icon);
        }
        /// <summary>
        /// 绘制地图图标
        /// </summary>
        private void DrawMapIcons()
        {
            if (!UI || !UI.gameObject) return;
            if (!MapCamera.orthographic) MapCamera.orthographic = true;
            if (!MapCamera.CompareTag("MapCamera")) MapCamera.tag = "MapCamera";
            if (MapCamera.cullingMask != mapRenderMask) MapCamera.cullingMask = mapRenderMask;
            foreach (var iconKvp in iconsWithHolder)
            {
                MapIconHolder holder = iconKvp.Key;
                float sqrDistance = Vector3.SqrMagnitude(holder.transform.position - player.position);
                if (iconKvp.Key.isActiveAndEnabled && !iconKvp.Value.ForceHided && (isViewingWorldMap && holder.drawOnWorldMap || !isViewingWorldMap && (!holder.AutoHide
                   || holder.AutoHide && holder.DistanceSqr >= sqrDistance)))
                {
                    if (holder.showRange && !iconKvp.Value.iconRange)
                        iconKvp.Value.iconRange = ObjectPool.Get(UI.rangePrefab.gameObject, UI.rangesParent).GetComponent<MapIconRange>();
                    holder.ShowIcon(CameraZoom);
                    DrawMapIcon(holder.transform.position + new Vector3(holder.offset.x, use2D ? holder.offset.y : 0, use2D ? 0 : holder.offset.y), iconKvp.Value, holder.keepOnMap);
                    if (!IsViewingWorldMap && sqrDistance > holder.DistanceSqr * 0.81f && sqrDistance < holder.DistanceSqr)
                        iconKvp.Value.ImageCanvas.alpha = (holder.DistanceSqr - sqrDistance) / (holder.DistanceSqr * 0.19f);
                    else iconKvp.Value.ImageCanvas.alpha = 1;
                }
                else holder.HideIcon();
            }
            foreach (var icon in IconsWithoutHolder.Values)
                DrawMapIcon(icon.worldPosition, icon.mapIcon, icon.keepOnMap);
        }
        /// <summary>
        /// 在相应的位置绘制图标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="icon"></param>
        /// <param name="keepOnMap"></param>
        private void DrawMapIcon(Vector3 worldPosition, MapIcon icon, bool keepOnMap)
        {
            if (!icon || !UI || !UI.gameObject) return;
            //把相机视野内的世界坐标归一化为一个裁剪正方体中的坐标，其边长为1，就是说所有视野内的坐标都变成了x、z、y分量都在(0,1)以内的裁剪坐标
            Vector3 viewportPoint = MapCamera.WorldToViewportPoint(worldPosition);
            //这一步用于修正UI因设备分辨率不一样，在进行缩放后实际Rect信息变了而产生的问题
            Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(UI.mapRect);
            //获取四个顶点的位置，顶点序号
            //  1 ┏━┓ 2
            //  0 ┗━┛ 3
            Vector3[] corners = new Vector3[4];
            UI.mapRect.GetWorldCorners(corners);
            //根据归一化的裁剪坐标，转化为相对于地图的坐标
            Vector3 screenPos = new Vector3(viewportPoint.x * screenSpaceRect.width + corners[0].x, viewportPoint.y * screenSpaceRect.height + corners[0].y, 0);
            Vector3 rangePos = screenPos;
            if (keepOnMap)
            {
                //以遮罩的Rect为范围基准而不是地图的
                screenSpaceRect = ZetanUtility.GetScreenSpaceRect(MapMaskRect);
                float size = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f;//地图的一半尺寸
                UI.mapWindowRect.GetWorldCorners(corners);
                if (circle && !isViewingWorldMap)
                {
                    //以下不使用UI.mapMaskRect.position，是因为该position值会受轴心(UI.mapMaskRect.pivot)位置的影响而使得最后的结果出现偏移
                    Vector3 realCenter = ZetanUtility.CenterBetween(corners[0], corners[2]);
                    Vector3 positionOffset = Vector3.ClampMagnitude(screenPos - realCenter, radius * size);
                    screenPos = realCenter + positionOffset;
                }
                else
                {
                    float edgeSize = (isViewingWorldMap ? worldEdgeSize : this.edgeSize) * size;
                    screenPos.x = Mathf.Clamp(screenPos.x, corners[0].x + edgeSize, corners[2].x - edgeSize);
                    screenPos.y = Mathf.Clamp(screenPos.y, corners[0].y + edgeSize, corners[1].y - edgeSize);
                }
            }
            icon.transform.position = screenPos;
            if (icon.iconRange) icon.iconRange.transform.position = rangePos;
        }
        /// <summary>
        /// 初始化玩家图标
        /// </summary>
        private void InitPlayerIcon()
        {
            if (playerIconInstance) return;
            playerIconInstance = ObjectPool.Get(UI.iconPrefab.gameObject, SelectParent(MapIconType.Main)).GetComponent<MapIcon>();
            playerIconInstance.iconImage.overrideSprite = playerIcon;
            playerIconInstance.iconImage.rectTransform.sizeDelta = playerIconSize;
            playerIconInstance.iconType = MapIconType.Main;
            playerIconInstance.iconImage.raycastTarget = false;
        }
        #endregion

        #region 地图切换相关
        /// <summary>
        /// 切换地图模式
        /// </summary>
        public void SwitchMapMode()
        {
            if (!UI || !UI.gameObject) return;
            isViewingWorldMap = !isViewingWorldMap;
            isMovingCamera = false;
            if (!isViewingWorldMap)//从大向小切换
            {
                if (animationSpeed > 0)
                {
                    UI.mapWindowRect.anchorMin = miniModeInfo.windowAnchoreMin;
                    UI.mapWindowRect.anchorMax = miniModeInfo.windowAnchoreMax;
                    UI.mapRect.anchorMin = miniModeInfo.mapAnchoreMin;
                    UI.mapRect.anchorMax = miniModeInfo.mapAnchoreMax;
                }
                else ToMiniMap();
            }
            else
            {
                if (animationSpeed > 0)
                {
                    UI.mapWindowRect.anchorMin = worldModeInfo.windowAnchoreMin;
                    UI.mapWindowRect.anchorMax = worldModeInfo.windowAnchoreMax;
                    UI.mapRect.anchorMin = worldModeInfo.mapAnchoreMin;
                    UI.mapRect.anchorMax = worldModeInfo.mapAnchoreMax;
                }
                else ToWorldMap();
            }
            if (animationSpeed > 0)
            {
                isSwitching = true;
                switchTime = 0;
                startSizeOfCamForMap = MapCamera.orthographicSize;
                startPosOfCamForMap = MapCamera.transform.position;
                startPositionOfMap = UI.mapWindowRect.anchoredPosition;
                startSizeOfMapWindow = UI.mapWindowRect.rect.size;
                startSizeOfMap = UI.mapRect.rect.size;
            }
            

        }
        /// <summary>
        /// 动画切换
        /// </summary>
        private void AnimateSwitching()
        {
            if (!UI || !UI.gameObject || !AnimateAble) return;
            switchTime += Time.deltaTime * animationSpeed;
            if (isViewingWorldMap)//从小向大切换
            {
                if (!IsAnimaComplete(true))
                {
                    AnimateTo(worldModeInfo);
                }
                else ToWorldMap();
            }
            else
            {
                if (!IsAnimaComplete(false))
                {
                    Vector3 newCamPos = new Vector3(player.position.x, use2D ? player.position.y : MapCamera.transform.position.y, use2D ? MapCamera.transform.position.z : player.position.z);
                    MapCamera.transform.position = Vector3.Lerp(startPosOfCamForMap, newCamPos, switchTime);
                    AnimateTo(miniModeInfo);
                }
                else ToMiniMap();
            }
        }
        /// <summary>
        /// 动画切换到指定动画信息
        /// </summary>
        /// <param name="modeInfo"></param>
        private void AnimateTo(MapModeInfo modeInfo)
        {
            if (!UI || !UI.gameObject) return;
            MapCamera.orthographicSize = Mathf.Lerp(startSizeOfCamForMap, modeInfo.currentSizeOfCam, switchTime);
            UI.mapWindowRect.anchoredPosition = Vector3.Lerp(startPositionOfMap, modeInfo.anchoredPosition, switchTime);
            UI.mapRect.sizeDelta = Vector2.Lerp(startSizeOfMap, modeInfo.sizeOfMap, switchTime);
            UI.mapWindowRect.sizeDelta = Vector2.Lerp(startSizeOfMapWindow, modeInfo.sizeOfWindow, switchTime);
        }
        /// <summary>
        /// 是否切换完成
        /// </summary>
        /// <param name="toWorldMode"></param>
        /// <returns></returns>
        private bool IsAnimaComplete(bool toWorldMode)
        {
            if (toWorldMode) return UI.mapRect.sizeDelta.x >= worldModeInfo.sizeOfMap.x && UI.mapRect.sizeDelta.y >= worldModeInfo.sizeOfMap.y;
            else return UI.mapRect.sizeDelta.x <= miniModeInfo.sizeOfMap.x && UI.mapRect.sizeDelta.y <= miniModeInfo.sizeOfMap.y;
        }
        /// <summary>
        /// 切换到小地图
        /// </summary>
        public void ToMiniMap()
        {
            isSwitching = false;
            isMovingCamera = false;
            switchTime = 0;
            cameraMovingTime = 0;
            isViewingWorldMap = false;
            SetInfoFrom(miniModeInfo);
        }
        /// <summary>
        /// 切换到大地图
        /// </summary>
        public void ToWorldMap()
        {
            isSwitching = false;
            isMovingCamera = false;
            switchTime = 0;
            cameraMovingTime = 0;
            isViewingWorldMap = true;
            SetInfoFrom(worldModeInfo);
        }
        /// <summary>
        /// s\设置信息
        /// </summary>
        /// <param name="modeInfo"></param>
        private void SetInfoFrom(MapModeInfo modeInfo)
        {
            if (!UI || !UI.gameObject) return;
            MapCamera.orthographicSize = modeInfo.currentSizeOfCam;
            zoomLimit.x = modeInfo.minZoomOfCam;
            zoomLimit.y = modeInfo.maxZoomOfCam;
            UI.mapWindowRect.anchorMin = modeInfo.windowAnchoreMin;
            UI.mapWindowRect.anchorMax = modeInfo.windowAnchoreMax;
            UI.mapRect.anchorMin = modeInfo.mapAnchoreMin;
            UI.mapRect.anchorMax = modeInfo.mapAnchoreMax;
            UI.mapWindowRect.anchoredPosition = modeInfo.anchoredPosition;
            UI.mapRect.sizeDelta = modeInfo.sizeOfMap;
            UI.mapWindowRect.sizeDelta = modeInfo.sizeOfWindow;
        }
        /// <summary>
        /// 将当前地图设置为小地图
        /// </summary>
        public void SetCurrentAsMiniMap()
        {
            if (!UI || !UI.gameObject || isViewingWorldMap) return;
            if (MapCamera)
            {
                miniModeInfo.defaultSizeOfCam = MapCamera.orthographicSize;
                miniModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
            }
            else Debug.LogError("地图相机不存在！");
            try
            {
                CopyInfoTo(miniModeInfo);
            }
            catch
            {
                Debug.LogError("地图UI不存在或UI存在空缺！");
            }
        }
        /// <summary>
        /// 将当前地图设置为大地图
        /// </summary>
        public void SetCurrentAsWorldMap()
        {
            if (!UI || !UI.gameObject || !isViewingWorldMap) return;
            if (MapCamera)
            {
                worldModeInfo.defaultSizeOfCam = MapCamera.orthographicSize;
                worldModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
            }
            else Debug.LogError("地图相机不存在！");
            try
            {
                CopyInfoTo(worldModeInfo);
            }
            catch
            {
                Debug.LogError("地图UI不存在或UI存在空缺！");
            }
        }
        /// <summary>
        /// 复制信息
        /// </summary>
        /// <param name="modeInfo"></param>
        private void CopyInfoTo(MapModeInfo modeInfo)
        {
            if (!UI || !UI.gameObject) return;
            modeInfo.windowAnchoreMin = UI.mapWindowRect.anchorMin;
            modeInfo.windowAnchoreMax = UI.mapWindowRect.anchorMax;
            modeInfo.mapAnchoreMin = UI.mapRect.anchorMin;
            modeInfo.mapAnchoreMax = UI.mapRect.anchorMax;
            modeInfo.anchoredPosition = UI.mapWindowRect.anchoredPosition;
            modeInfo.sizeOfWindow = UI.mapWindowRect.sizeDelta;
            modeInfo.sizeOfMap = UI.mapRect.sizeDelta;
        }
        #endregion

        #region 相机相关
        /// <summary>
        /// 设置玩家
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Transform player)
        {
            this.player = player;
        }
        /// <summary>
        /// 重新产生相机
        /// </summary>
        public void RemakeCamera()
        {
            if (!cameraPrefab) return;
            if (mapCamera) ObjectPool.Put(mapCamera);
            mapCamera = Instantiate(cameraPrefab, transform);
            mapCamera.Camera.targetTexture = targetTexture;
        }
        /// <summary>
        /// 相机跟随
        /// </summary>
        private void FollowPlayer()
        {
            if (!player || !playerIconInstance) return;
            DrawMapIcon(isViewingWorldMap ? player.position : MapCamera.transform.position, playerIconInstance, true);
            playerIconInstance.transform.SetSiblingIndex(playerIconInstance.transform.childCount - 1);
            if (!rotateMap)
            {
                if (use2D)
                {
                    playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, player.eulerAngles.z);
                    MapCamera.transform.eulerAngles = Vector3.zero;
                }
                else
                {
                    playerIconInstance.transform.eulerAngles = new Vector3(playerIconInstance.transform.eulerAngles.x, playerIconInstance.transform.eulerAngles.y, -player.eulerAngles.y);
                    MapCamera.transform.eulerAngles = Vector3.right * 90;
                }
            }
            else
            {
                if (use2D)
                {
                    playerIconInstance.transform.eulerAngles = Vector3.zero;
                    MapCamera.transform.eulerAngles = new Vector3(0, 0, player.eulerAngles.z);
                }
                else
                {
                    playerIconInstance.transform.eulerAngles = Vector3.zero;
                    MapCamera.transform.eulerAngles = new Vector3(MapCamera.transform.eulerAngles.x, player.eulerAngles.y, MapCamera.transform.eulerAngles.z);
                }
            }
            if (!isViewingWorldMap && !isSwitching && !isMovingCamera)
            {
                //2D模式，则跟随目标的Y坐标，相机的Z坐标不动；3D模式，则跟随目标的Z坐标，相机的Y坐标不动。
                Vector3 newCamPos = new Vector3(player.position.x + offset.x,
                    use2D ? player.position.y + offset.y : MapCamera.transform.position.y,
                    use2D ? MapCamera.transform.position.z : player.position.z + offset.y);
                MapCamera.transform.position = newCamPos;
            }
            playerIconInstance.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 定位玩家
        /// </summary>
        public void LocatePlayer()
        {
            MoveCameraTo(player.position);
        }
        /// <summary>
        /// 移动相机
        /// </summary>
        /// <param name="worldPosition"></param>
        public void MoveCameraTo(Vector3 worldPosition)
        {
            if (isSwitching || !isViewingWorldMap) return;
            Vector3 newCamPos = new Vector3(worldPosition.x, use2D ? worldPosition.y : MapCamera.transform.position.y, use2D ? MapCamera.transform.position.z : worldPosition.z);
            startPosOfCamForMap = MapCamera.transform.position;
            camMoveDestination = newCamPos;
            isMovingCamera = true;
            cameraMovingTime = 0;
        }
        /// <summary>
        /// 地图中的位置转换为世界坐标
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        public Vector3 MapPointToWorldPoint(Vector3 mousePosition)
        {
            Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(UI.mapRect);
            Vector3[] corners = new Vector3[4];
            UI.mapRect.GetWorldCorners(corners);
            Vector2 mapViewportPoint = new Vector2((mousePosition.x - corners[0].x) / screenSpaceRect.width, (mousePosition.y - corners[0].y) / screenSpaceRect.height);
            Vector3 worldPosition = MapCamera.ViewportToWorldPoint(mapViewportPoint);
            return use2D ? new Vector3(worldPosition.x, worldPosition.y) : worldPosition;
        }
        /// <summary>
        /// 在世界地图拖拽逻辑
        /// </summary>
        /// <param name="direction"></param>
        public void DragWorldMap(Vector2 direction)
        {
            if (!isViewingWorldMap || isSwitching) return;
            if (direction == default) return;
            isMovingCamera = false;
            cameraMovingTime = 0;
            float mag = new Vector2(Screen.width, Screen.height).magnitude;
            direction = new Vector2(direction.x * 1000 / mag, direction.y * 1000 / mag) * (MapCamera.orthographicSize / worldModeInfo.currentSizeOfCam);
            MapCamera.transform.Translate(new Vector3(direction.x, use2D ? direction.y : 0, use2D ? 0 : direction.y) * dragSensitivity / CameraZoom);
        }
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="value"></param>
        public void Zoom(float value)
        {
            if (isSwitching || value == 0) return;
            MapCamera.orthographicSize = Mathf.Clamp(MapCamera.orthographicSize - value, zoomLimit.x, zoomLimit.y);
            if (IsViewingWorldMap) worldModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
            else miniModeInfo.currentSizeOfCam = MapCamera.orthographicSize;
        }

        public float CameraZoom => IsViewingWorldMap ? (worldModeInfo.defaultSizeOfCam / MapCamera.orthographicSize) : (miniModeInfo.defaultSizeOfCam / MapCamera.orthographicSize);
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (updateMode == UpdateMode.Update) DrawMapIcons();
            if (isSwitching) AnimateSwitching();
            if (isMovingCamera)
            {
                cameraMovingTime += Time.deltaTime * 5;
                if (camMoveDestination != MapCamera.transform.position)
                    MapCamera.transform.position = Vector3.Lerp(startPosOfCamForMap, camMoveDestination, cameraMovingTime);
                else
                {
                    isMovingCamera = false;
                    cameraMovingTime = 0;
                }
            }
        }

        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate) DrawMapIcons();
        }
        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate) DrawMapIcons();
            FollowPlayer();
        }

        private void OnDrawGizmos()
        {
            if (!UI || !UI.gameObject || !MapMaskRect) return;
            Rect screenSpaceRect = ZetanUtility.GetScreenSpaceRect(MapMaskRect);
            Vector3[] corners = new Vector3[4];
            UI.mapMaskRect.GetWorldCorners(corners);
            if (circle && !isViewingWorldMap)
            {
                float radius = (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height) * 0.5f * this.radius;
                //ZetanUtility.DrawGizmosCircle(ZetanUtility.CenterBetween(corners[0], corners[2]), radius, radius / 1000, Color.white, false);
                ZetanUtility.DrawGizmosCircle(ZetanUtility.CenterBetween(corners[0], corners[2]), radius, Vector3.forward);
            }
            else
            {
                float edgeSize = isViewingWorldMap ? worldEdgeSize : this.edgeSize;
                Vector3 size = new Vector3(screenSpaceRect.width - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height),
                    screenSpaceRect.height - edgeSize * (screenSpaceRect.width < screenSpaceRect.height ? screenSpaceRect.width : screenSpaceRect.height), 0);
                Gizmos.DrawWireCube(ZetanUtility.CenterBetween(corners[0], corners[2]), size);
            }
        }
        #endregion

        #region 其它
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InitPlayerIcon();
            if (targetTexture) targetTexture.Release();
            targetTexture = new RenderTexture(textureSize.x, textureSize.y, 24, textureFormat)
            {
                name = "MapTexture"
            };
            MapCamera.targetTexture = targetTexture;
            UI.mapImage.texture = targetTexture;
            miniModeInfo.currentSizeOfCam = miniModeInfo.defaultSizeOfCam;
            worldModeInfo.currentSizeOfCam = worldModeInfo.defaultSizeOfCam;
            ToMiniMap();
        }
        /// <summary>
        /// 清除所有的标志
        /// </summary>
        private void ClearMarks()
        {
            MapIconWithoutHolder[] iconWoHs = new MapIconWithoutHolder[IconsWithoutHolder.Count];
            IconsWithoutHolder.Values.CopyTo(iconWoHs, 0);
            foreach (var iconWoH in iconWoHs)
                if (iconWoH.mapIcon.iconType == MapIconType.Mark) RemoveMapIcon(iconWoH.mapIcon, true);
        }
        /// <summary>
        /// 清除所有标志
        /// </summary>
        public void ClearAllMarks()
        {
            UI.questsParent.transform.CollectAllChild();
            UI.objectivesParent.transform.DeleteAllChild();
        }
        /// <summary>
        /// 选择父物体
        /// </summary>
        /// <param name="iconType"></param>
        /// <returns></returns>
        private RectTransform SelectParent(MapIconType iconType)
        {
            switch (iconType)
            {
                case MapIconType.Normal:
                    return UI.iconsParent;
                case MapIconType.Main:
                    return UI.mainParent;
                case MapIconType.Mark:
                    return UI.marksParent;
                case MapIconType.Quest:
                    return UI.questsParent;
                case MapIconType.Objective:
                    return UI.objectivesParent;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 打开窗口
        /// </summary>
        public void OpenWindow()
        {
            UI.transform.FindChildComponentByName<CanvasGroup>("MapWindow").alpha = 1;
            UI.transform.FindChildComponentByName<CanvasGroup>("MapWindow").blocksRaycasts = true;
            FindObjectOfType<ScenePanel>(true).mapButton.gameObject.SetActive(false);
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseWindow()
        {
            UI.transform.FindChildComponentByName<CanvasGroup>("MapWindow").alpha = 0;
            UI.transform.FindChildComponentByName<CanvasGroup>("MapWindow").blocksRaycasts = false;
            FindObjectOfType<ScenePanel>(true).mapButton.gameObject.SetActive(true);
        }
        /// <summary>
        /// 保存图标数据
        /// </summary>
        /// <param name="data"></param>
        public void SaveData(SaveData data)
        {
            foreach (var iconWoH in IconsWithoutHolder.Values)
                if (iconWoH.mapIcon.iconType == MapIconType.Mark) data.markDatas.Add(new MapMarkSaveData(iconWoH));
        }
        /// <summary>
        /// 读取图标数据
        /// </summary>
        /// <param name="data"></param>
        public void LoadData(SaveData data)
        {
            ClearMarks();
            foreach (var md in data.markDatas)
                CreateDefaultMark(new Vector3(md.worldPosX, md.worldPosY, md.worldPosZ), md.keepOnMap, md.removeAble, md.textToDisplay);
        }

        public class MapIconWithoutHolder
        {
            public Vector3 worldPosition;
            public MapIcon mapIcon;
            public bool keepOnMap;
            public bool removeAble;
            public string textToDisplay;

            public MapIconWithoutHolder(Vector3 worldPosition, MapIcon mapIcon, bool keepOnMap, bool removeAble, string textToDisplay)
            {
                this.worldPosition = worldPosition;
                this.mapIcon = mapIcon;
                this.keepOnMap = keepOnMap;
                this.removeAble = removeAble;
                this.textToDisplay = textToDisplay;
            }
        }

        [Serializable]
        public class MapModeInfo
        {
            public float defaultSizeOfCam;
            public float currentSizeOfCam;
            public float minZoomOfCam;
            public float maxZoomOfCam;
            public Vector2 windowAnchoreMin;
            public Vector2 windowAnchoreMax;
            public Vector2 mapAnchoreMin;
            public Vector2 mapAnchoreMax;
            public Vector2 anchoredPosition;
            public Vector2 sizeOfWindow;
            public Vector2 sizeOfMap;
        }
        #endregion
    }
}