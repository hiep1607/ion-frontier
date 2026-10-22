using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    public sealed partial class IonHud
    {
        GameObject draftPanel;Text draftContext,draftFooter;
        readonly GameObject[] modulePanels=new GameObject[2];
        readonly IonModuleIcon[] moduleIcons=new IonModuleIcon[2],draftModuleIcons=new IonModuleIcon[3];
        readonly Text[] moduleTitles=new Text[2],moduleStates=new Text[2];float nextModuleHud;
        readonly Button[] draftButtons=new Button[3];
        readonly Text[] draftTitles=new Text[3],draftDescriptions=new Text[3];
        readonly Image[] draftIcons=new Image[3];
        void MakeDraftInterface()
        {
            for(int i=0;i<2;i++)
            {
                var panel=Bar(flight.transform,new Vector2(i==0?0:1,0),new Vector2(i==0?235:-235,190),new Vector2(450,64),new Color(.015f,.04f,.065f,.4f));modulePanels[i]=panel.gameObject;
                var symbol=new GameObject("Module symbol",typeof(RectTransform)).AddComponent<IonModuleIcon>();symbol.transform.SetParent(panel.transform,false);symbol.rectTransform.sizeDelta=new Vector2(42,42);symbol.rectTransform.anchoredPosition=new Vector2(-192,0);symbol.color=IonArt.Gold;symbol.raycastTarget=false;moduleIcons[i]=symbol;
                moduleTitles[i]=Label("",panel.transform,17,text,Vector2.one*.5f,new Vector2(22,14),new Vector2(365,24),TextAnchor.MiddleLeft);
                moduleStates[i]=Label("",panel.transform,16,IonArt.Cyan,Vector2.one*.5f,new Vector2(22,-13),new Vector2(365,24),TextAnchor.MiddleLeft);panel.gameObject.SetActive(false);
            }
            draftPanel=Panel("Checkpoint upgrade choice",transform,new Color(.008f,.018f,.035f,.97f));
            Label("CHỌN NÂNG CẤP",draftPanel.transform,40,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,335),new Vector2(1400,70),TextAnchor.MiddleCenter);
            draftContext=Label("",draftPanel.transform,18,text,Vector2.one*.5f,new Vector2(0,255),new Vector2(1400,90),TextAnchor.MiddleCenter);
            for(int i=0;i<3;i++)
            {
                int card=i;var button=Button("",draftPanel.transform,Vector2.one*.5f,new Vector2((i-1)*430,0),new Vector2(400,430),()=>game.ChooseUpgrade(card),false);
                draftButtons[i]=button.GetComponent<Button>();
                draftIcons[i]=Bar(button.transform,Vector2.one*.5f,new Vector2(0,151),new Vector2(64,64),IonArt.Gold);draftIcons[i].preserveAspect=true;
                var moduleIcon=new GameObject("Draft module symbol",typeof(RectTransform)).AddComponent<IonModuleIcon>();moduleIcon.transform.SetParent(button.transform,false);moduleIcon.rectTransform.anchoredPosition=new Vector2(0,151);moduleIcon.rectTransform.sizeDelta=new Vector2(64,64);moduleIcon.color=IonArt.Gold;moduleIcon.raycastTarget=false;draftModuleIcons[i]=moduleIcon;
                draftTitles[i]=Label("",button.transform,23,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,80),new Vector2(360,65),TextAnchor.MiddleCenter);draftTitles[i].horizontalOverflow=HorizontalWrapMode.Wrap;
                draftDescriptions[i]=Label("",button.transform,18,text,Vector2.one*.5f,new Vector2(0,-51),new Vector2(350,180),TextAnchor.UpperCenter);draftDescriptions[i].horizontalOverflow=HorizontalWrapMode.Wrap;
                Label((i+1)+" / CHỌN",button.transform,18,IonArt.Gold,Vector2.one*.5f,new Vector2(0,-179),new Vector2(350,36),TextAnchor.MiddleCenter);
            }
            draftFooter=Label("",draftPanel.transform,19,IonArt.Gold,Vector2.one*.5f,new Vector2(0,-268),new Vector2(1400,50),TextAnchor.MiddleCenter);
            Label("Mỗi checkpoint nhận một lựa chọn • P / ESC tạm dừng",draftPanel.transform,16,muted,Vector2.one*.5f,new Vector2(0,-325),new Vector2(1400,40),TextAnchor.MiddleCenter);
            draftPanel.SetActive(false);
        }
        public void TestClickDraftCard(int slot){if(slot>=0&&slot<3)draftButtons[slot].onClick.Invoke();}
        public void TestModuleLegend()
        {
            var gallery=Panel("QA module symbols",transform,new Color(.008f,.018f,.035f,.99f));Label("CHÍN MÔ-ĐUN CHIẾN ĐẤU",gallery.transform,32,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,370),new Vector2(1400,60),TextAnchor.MiddleCenter);
            for(int i=0;i<9;i++)
            {
                Vector2 p=new Vector2((i%3-1)*450,220-i/3*210);var panel=Bar(gallery.transform,Vector2.one*.5f,p,new Vector2(410,178),new Color(.03f,.065f,.095f));var symbol=new GameObject("Module icon",typeof(RectTransform)).AddComponent<IonModuleIcon>();symbol.transform.SetParent(panel.transform,false);symbol.rectTransform.sizeDelta=new Vector2(68,68);symbol.rectTransform.anchoredPosition=new Vector2(0,25);symbol.SetModule(i);symbol.color=IonArt.Gold;symbol.raycastTarget=false;Label(IonGame.ModuleName((CombatModule)i),panel.transform,21,text,Vector2.one*.5f,new Vector2(0,-45),new Vector2(395,44),TextAnchor.MiddleCenter);
            }
        }
        public void TickDraftInterface()
        {
            if(draftPanel==null)return;bool show=game.DraftActive&&game.State!=FlightState.Paused;draftPanel.SetActive(show);
            if(Time.unscaledTime>=nextModuleHud)
            {
                nextModuleHud=Time.unscaledTime+.08f;
                for(int i=0;i<2;i++){var module=game.EquippedModule(i);bool equipped=(int)module>=0;modulePanels[i].SetActive(equipped);if(!equipped)continue;moduleIcons[i].SetModule((int)module);moduleTitles[i].text=IonGame.ModuleName(module);moduleStates[i].text=game.ModuleStatus(module);moduleStates[i].color=game.ModuleActive(module)?IonArt.Cyan:IonArt.Orange;}
            }
            if(!show)return;draftPanel.transform.SetAsLastSibling();
            draftContext.text=(game.TestDraftInstruction==""?"":game.TestDraftInstruction+"\n")+"CHECKPOINT "+game.DraftOpenCount+" / 2  •  "+game.Loadout.Summary+"\n"+game.ModuleSummary;
            draftFooter.text=game.DraftResumeText;
            for(int i=0;i<3;i++)
            {
                draftTitles[i].text=game.DraftCardTitle(i);draftDescriptions[i].text=game.DraftCardDescription(i);draftIcons[i].sprite=game.DraftCardIcon(i);
                int moduleId=game.DraftCardModule(i);draftIcons[i].gameObject.SetActive(moduleId<0);draftModuleIcons[i].gameObject.SetActive(moduleId>=0);draftModuleIcons[i].SetModule(moduleId);
                draftButtons[i].interactable=game.DraftSelected<0;draftButtons[i].image.color=game.DraftSelected==i?new Color(.12f,.32f,.37f):new Color(.035f,.075f,.11f);
            }
        }
    }
}
