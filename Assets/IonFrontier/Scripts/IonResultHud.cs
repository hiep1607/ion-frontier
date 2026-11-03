using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IonFrontier
{
    public sealed partial class IonHud
    {
        const float ResultStarStart=.35f,ResultStarGap=.42f,ResultSequenceEnd=2.5f;
        RectTransform resultFrame,resultShipRect,rewardShipRect;
        Text resultShipName,resultRunLabel,resultRewardBadge,resultScore,resultHint;
        Text rewardName,rewardRole,rewardDescription;
        readonly CanvasGroup[] resultRowGroups=new CanvasGroup[5];
        readonly float[] resultStarPops=new float[5];
        CanvasGroup resultActions,resultBaseInteraction,rewardContents;
        Button resultSkip,resultNext,resultReplay,resultHome,rewardKeep,rewardUse;
        GameObject rewardModal;Image rewardShip,rewardGlow,rewardRing;
        int shownResultSerial=-1,resultGoldStars,resultRowsShown;float resultClock,rewardClock;
        bool resultSequenceDone,rewardWasShown;
        bool ResultVisible=>result!=null&&result.activeSelf;
        public bool NewHullRevealOpen=>rewardModal!=null&&rewardModal.activeSelf;
        void MakeRewardResult()
        {
            result=Panel("Debrief",transform,new Color(.01f,.025f,.045f,.975f));
            resultFrame=new GameObject("Result design frame",typeof(RectTransform),typeof(CanvasGroup)).GetComponent<RectTransform>();resultFrame.SetParent(result.transform,false);resultFrame.anchorMin=resultFrame.anchorMax=Vector2.one*.5f;resultFrame.sizeDelta=new Vector2(1600,900);resultBaseInteraction=resultFrame.GetComponent<CanvasGroup>();
            var p=resultFrame;
            MLabel("IF / BÁO CÁO CHUYẾN BAY",p,75,35,1100,35,19,IonArt.Cyan);
            statusTitle=MLabel("",p,72,100,1450,75,42,text);statusBody=MLabel("",p,75,180,1450,55,19,MenuMuted);
            MBox(p,75,251,1450,1,MenuLine);
            MLabel("CHIẾN HẠM VỪA THAM CHIẾN",p,78,280,485,35,17,IonArt.Cyan);
            unlockImage=MPicture(p,105,335,405,322,null);resultShipRect=unlockImage.rectTransform;
            resultShipName=MLabel("",p,75,675,500,47,32,text,TextAnchor.MiddleCenter);
            resultRunLabel=MLabel("",p,80,728,490,58,18,MenuMuted,TextAnchor.MiddleCenter);
            resultRewardBadge=MLabel("",p,80,799,490,40,18,IonArt.Gold,TextAnchor.MiddleCenter);
            ratingBoard=MGroup("Five-star debrief",p);
            for(int i=0;i<5;i++)
            {
                ratingStars[i]=Star(ratingBoard.transform,Vector2.zero,62,MenuLine);var rt=ratingStars[i].rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.anchoredPosition=new Vector2(780+i*130,-304);
                var row=MBox(ratingBoard.transform,645,410+i*62,860,56,new Color(.045f,.085f,.12f));resultRowGroups[i]=row.gameObject.AddComponent<CanvasGroup>();
                ratingRows[i]=MLabel("",row.transform,18,4,660,26,18,text);ratingValues[i]=MLabel("",row.transform,18,30,690,23,15,MenuMuted);
                ratingStates[i]=MLabel("",row.transform,716,11,130,34,16,MenuMuted,TextAnchor.MiddleCenter);
            }
            ratingSummary=MLabel("",ratingBoard.transform,645,355,860,42,23,IonArt.Gold,TextAnchor.MiddleCenter);
            resultScore=MLabel("",p,650,728,855,35,20,text);
            var actions=MGroup("Result navigation actions",p);resultActions=actions.AddComponent<CanvasGroup>();
            resultNext=MButton("Result.Next","MÀN TIẾP →",actions.transform,650,788,410,60,ResultNext,true);nextButton=resultNext.gameObject;
            resultReplay=MButton("Result.Replay","CHƠI LẠI",actions.transform,1080,788,230,60,ResultReplay);replayButton=resultReplay.gameObject;
            resultHome=MButton("Result.Home","CĂN CỨ",actions.transform,1330,788,175,60,ResultHome);
            resultSkip=MButton("Result.Skip","HIỆN KẾT QUẢ NGAY →",p,1080,788,425,60,CompleteResultSequence);
            resultHint=MLabel("",p,650,858,855,28,15,MenuMuted);
            MakeNewHullReveal();
        }
        void MakeNewHullReveal()
        {
            rewardModal=Panel("New hull reveal",result.transform,new Color(.007f,.018f,.04f,1));rewardModal.GetComponent<Image>().raycastTarget=true;
            var frame=new GameObject("New hull reveal frame",typeof(RectTransform),typeof(CanvasGroup)).GetComponent<RectTransform>();frame.SetParent(rewardModal.transform,false);frame.anchorMin=frame.anchorMax=Vector2.one*.5f;frame.sizeDelta=new Vector2(1600,900);rewardContents=frame.GetComponent<CanvasGroup>();
            rewardGlow=MPicture(frame,480,110,640,640,IonArt.Glow);rewardGlow.color=new Color(.1f,.6f,.95f,.35f);
            rewardRing=MPicture(frame,560,190,480,480,IonArt.Ring);rewardRing.color=new Color(.2f,.7f,.95f,.4f);
            MLabel("CHIẾN HẠM MỚI ĐÃ MỞ KHÓA",frame,150,65,1300,65,28,IonArt.Gold,TextAnchor.MiddleCenter);
            MLabel("PHẦN THƯỞNG HOÀN THÀNH NHIỆM VỤ",frame,150,130,1300,35,17,MenuMuted,TextAnchor.MiddleCenter);
            rewardShip=MPicture(frame,580,220,440,400,null);rewardShipRect=rewardShip.rectTransform;
            rewardName=MLabel("",frame,150,625,1300,60,42,text,TextAnchor.MiddleCenter);
            rewardRole=MLabel("",frame,150,690,1300,35,19,IonArt.Cyan,TextAnchor.MiddleCenter);
            rewardDescription=MLabel("",frame,250,733,1100,38,18,MenuMuted,TextAnchor.MiddleCenter);
            rewardKeep=MButton("Result.Reward.Keep","GIỮ TÀU HIỆN TẠI",frame,340,806,445,62,()=>DismissNewHull(false));
            rewardUse=MButton("Result.Reward.Use","DÙNG TỪ CHUYẾN SAU →",frame,815,806,445,62,()=>DismissNewHull(true),true);
            rewardModal.SetActive(false);
        }
        void RefreshRewardResult()
        {
            bool clear=game.State==FlightState.StageClear,won=game.State==FlightState.Victory,visible=clear||won||game.State==FlightState.Defeat;
            result.SetActive(visible);if(!visible){rewardModal.SetActive(false);resultBaseInteraction.interactable=resultBaseInteraction.blocksRaycasts=true;return;}
            ratingBoard.SetActive(clear||won);resultNext.gameObject.SetActive(clear);
            unlockImage.gameObject.SetActive(true);unlockImage.sprite=IonArt.PlayerShips[game.LastCompletedHull];
            resultShipName.text=IonCampaign.HullNames[game.LastCompletedHull];resultRunLabel.text=IonHullStats.Roles[game.LastCompletedHull]+"\nMÀN "+game.Sector+" / "+game.SectorName;
            statusTitle.text=clear?"HOÀN THÀNH MÀN "+game.Sector:won?"PHÁ VỠ ĐẾ HẠM":"MẤT TÍN HIỆU";
            statusBody.text=game.LastRunWon?(game.LastOpenedStage>0?"ĐÃ MỞ MÀN "+game.LastOpenedStage:won?"HOÀN TẤT CHIẾN DỊCH 13 MÀN":"HOÀN THÀNH CHUYẾN BAY"):(game.DefeatReason+" Tiến trình mở khóa được giữ.");
            resultRewardBadge.text=game.LastNewHull>=0?"TÀU MỚI / "+IonCampaign.HullNames[game.LastNewHull]:"";
            for(int i=0;i<5;i++){ratingRows[i].text=game.RatingCriterionName(i);ratingValues[i].text=game.LastRatingDetails[i];}
            if(shownResultSerial!=game.ResultSerial)
            {
                shownResultSerial=game.ResultSerial;resultClock=0;resultGoldStars=resultRowsShown=0;resultSequenceDone=!game.LastRunWon;rewardWasShown=false;rewardModal.SetActive(false);
                for(int i=0;i<5;i++){resultStarPops[i]=-100;ratingStars[i].color=MenuLine;ratingStars[i].rectTransform.localScale=Vector3.one;resultRowGroups[i].alpha=0;ratingStates[i].text="";}
                RefreshResultNavigation();if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(resultSequenceDone?(clear?resultNext:resultReplay).gameObject:resultSkip.gameObject);
            }
            LayoutRewardResult();RenderResultSequence();
        }
        void LayoutRewardResult()
        {
            float scale=Mathf.Min(((RectTransform)transform).rect.width/1600,((RectTransform)transform).rect.height/900);resultFrame.localScale=Vector3.one*scale;
            if(rewardContents!=null)((RectTransform)rewardContents.transform).localScale=Vector3.one*scale;
        }
        void TickRewardResult()
        {
            if(!ResultVisible)return;LayoutRewardResult();
            if(NewHullRevealOpen)
            {
                rewardClock+=Mathf.Min(Time.unscaledDeltaTime,.1f);RenderNewHullReveal();EnsureModalFocus(rewardModal,rewardKeep);return;
            }
            if(!resultSequenceDone){resultClock+=Mathf.Min(Time.unscaledDeltaTime,.1f);RenderResultSequence();if(resultClock>=ResultSequenceEnd)CompleteResultSequence();}
        }
        void RenderResultSequence()
        {
            if(!game.LastRunWon){resultScore.text="ĐIỂM "+game.Score.ToString("D6")+"    /    KỶ LỤC "+game.Best.ToString("D6");return;}
            int reveal=Mathf.Clamp(Mathf.FloorToInt((resultClock-ResultStarStart)/ResultStarGap)+1,0,5);
            for(int i=resultRowsShown;i<reveal;i++)
            {
                bool earned=game.LastRatingEarned(i);ratingStates[i].text=earned?"+1 SAO":"CHƯA ĐẠT";ratingStates[i].color=earned?IonArt.Gold:MenuMuted;
                if(earned){resultStarPops[resultGoldStars]=resultClock;resultGoldStars++;if(!ReducedMenuMotion&&!resultSequenceDone)game.PlayResultCue(resultGoldStars);}
            }
            resultRowsShown=Mathf.Max(resultRowsShown,reveal);
            for(int i=0;i<5;i++)
            {
                resultRowGroups[i].alpha=i<reveal?Mathf.Clamp01((resultClock-ResultStarStart-i*ResultStarGap)/.18f):0;
                ratingStars[i].color=i<resultGoldStars?IonArt.Gold:MenuLine;
                float age=resultClock-resultStarPops[i],scale=1;
                if(!ReducedMenuMotion&&i<resultGoldStars&&age<.32f){float t=Mathf.Clamp01(age/.32f);scale=t<.6f?Mathf.Lerp(.45f,1.16f,Mathf.SmoothStep(0,1,t/.6f)):Mathf.Lerp(1.16f,1,Mathf.SmoothStep(0,1,(t-.6f)/.4f));}
                ratingStars[i].rectTransform.localScale=Vector3.one*scale;
            }
            ratingSummary.text=resultGoldStars+" / 5 SAO"+(resultSequenceDone?"    ·    TỐT NHẤT "+game.StarsAt(game.Sector)+" / 5"+(game.LastWasNewRating?"    ·    KỶ LỤC MỚI":""):"    /    ĐANG TỔNG KẾT");
            int count=Mathf.RoundToInt(game.Score*Mathf.Clamp01(resultClock/ResultSequenceEnd));resultScore.text="ĐIỂM "+count.ToString("D6")+"    /    KỶ LỤC "+game.Best.ToString("D6");
        }
        void CompleteResultSequence()
        {
            if(!ResultVisible||NewHullRevealOpen)return;
            resultClock=ResultSequenceEnd;resultSequenceDone=true;RenderResultSequence();for(int i=0;i<5;i++)resultStarPops[i]=-100;RenderResultSequence();RefreshResultNavigation();
            if(game.LastRunWon&&game.LastNewHull>=0&&!rewardWasShown)OpenNewHullReveal();
            else if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject((game.State==FlightState.StageClear?resultNext:resultReplay).gameObject);
        }
        void RefreshResultNavigation()
        {
            resultActions.alpha=resultSequenceDone?1:0;resultActions.interactable=resultActions.blocksRaycasts=resultSequenceDone&&!NewHullRevealOpen;
            resultSkip.gameObject.SetActive(!resultSequenceDone);resultHint.text=resultSequenceDone?"R / CHƠI LẠI    ·    ESC / CĂN CỨ":"ENTER / HIỆN ĐỦ KẾT QUẢ    ·    Phần thưởng đã được lưu";
            if(game.State!=FlightState.StageClear){MRect(resultReplay,650,788,500,60);MRect(resultHome,1170,788,335,60);}else{MRect(resultReplay,1080,788,230,60);MRect(resultHome,1330,788,175,60);}
        }
        void OpenNewHullReveal()
        {
            rewardWasShown=true;rewardClock=0;rewardShip.sprite=IonArt.PlayerShips[game.LastNewHull];rewardName.text=IonCampaign.HullNames[game.LastNewHull];rewardRole.text=IonHullStats.Roles[game.LastNewHull];rewardDescription.text="Q / "+game.MenuUltimateName(game.LastNewHull);
            rewardModal.SetActive(true);resultBaseInteraction.interactable=false;resultBaseInteraction.blocksRaycasts=false;rewardKeep.interactable=rewardUse.interactable=false;RenderNewHullReveal();
            if(!ReducedMenuMotion)game.PlayResultCue(0,true);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
        }
        void RenderNewHullReveal()
        {
            float t=ReducedMenuMotion?1:Mathf.Clamp01(rewardClock/.8f);float eased=1-Mathf.Pow(1-t,3);
            rewardContents.alpha=ReducedMenuMotion?1:Mathf.Clamp01(rewardClock/.25f);rewardShipRect.localScale=Vector3.one*Mathf.Lerp(.7f,1,eased);rewardShip.color=Color.Lerp(new Color(.02f,.08f,.13f),Color.white,eased);
            rewardGlow.rectTransform.localScale=Vector3.one*(ReducedMenuMotion?1:Mathf.Lerp(.7f,1.12f,eased));rewardRing.rectTransform.localScale=Vector3.one*(ReducedMenuMotion?1:Mathf.Lerp(.65f,1.2f,eased));rewardRing.color=new Color(.2f,.7f,.95f,Mathf.Lerp(.6f,.18f,eased));
            rewardKeep.interactable=rewardUse.interactable=rewardClock>=.85f;
        }
        void DismissNewHull(bool equip)
        {
            if(!NewHullRevealOpen||rewardClock<.85f)return;
            if(equip)game.QueueNewResultHull();rewardModal.SetActive(false);resultBaseInteraction.interactable=resultBaseInteraction.blocksRaycasts=true;RefreshResultNavigation();
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject((game.State==FlightState.StageClear?resultNext:resultReplay).gameObject);
        }
        void ResultNext(){if(resultSequenceDone&&!NewHullRevealOpen)game.NextStageHangar();}
        void ResultReplay(){if(resultSequenceDone&&!NewHullRevealOpen)game.BeginRun();}
        void ResultHome(){if(resultSequenceDone&&!NewHullRevealOpen)game.ReturnToMenu();}
        public bool HandleResultInput()
        {
            if(!ResultVisible)return false;
            if(Input.GetKeyDown(KeyCode.M))game.ToggleAudio();
            if(Input.GetKeyDown(KeyCode.Escape)){if(NewHullRevealOpen)DismissNewHull(false);else if(!resultSequenceDone)CompleteResultSequence();else ResultHome();}
            if(Input.GetKeyDown(KeyCode.R)){if(!resultSequenceDone)CompleteResultSequence();else if(!NewHullRevealOpen)ResultReplay();}
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                var buttons=NewHullRevealOpen?rewardModal.GetComponentsInChildren<Button>():resultFrame.GetComponentsInChildren<Button>();int at=System.Array.FindIndex(buttons,b=>EventSystem.current!=null&&b.gameObject==EventSystem.current.currentSelectedGameObject);int direction=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?-1:1;
                for(int n=0;n<buttons.Length;n++){at=(at+direction+buttons.Length)%buttons.Length;if(buttons[at].IsInteractable()){EventSystem.current.SetSelectedGameObject(buttons[at].gameObject);break;}}
            }
            return true;
        }
    }
}
