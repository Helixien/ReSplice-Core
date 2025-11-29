using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReSpliceCore
{
    [HotSwappable]
    public class Window_CombineGene : GeneCreationDialogBase
    {
        private Building_GeneCentrifuge centrifuge;
        private List<Genepack> allGenepacks;
        private List<Genepack> selectedGenepacks = new List<Genepack>();
        private HashSet<Genepack> matchingGenepacks = new HashSet<Genepack>();
        
        private List<GeneDef> tmpGenes = new List<GeneDef>();

        public override Vector2 InitialSize => new Vector2(1016f, 550f);

        public override string Header => "RS.CombineGenepacks".Translate();

        public override string AcceptButtonLabel => "RS.Combine".Translate();

        public override List<GeneDef> SelectedGenes
        {
            get
            {
                tmpGenes.Clear();
                foreach (Genepack selectedGenepack in selectedGenepacks)
                {
                    foreach (GeneDef item in selectedGenepack.GeneSet.GenesListForReading)
                    {
                        tmpGenes.Add(item);
                    }
                }
                return tmpGenes;
            }
        }

        public Window_CombineGene(Building_GeneCentrifuge centrifuge)
        {
            this.centrifuge = centrifuge;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            doCloseX = true;
            
            allGenepacks = RS_Utils.GetAllGenepacks(centrifuge.Map);
            allGenepacks.SortGenepacks();
        }

        public override void Accept()
        {
            if (selectedGenepacks.Count == 2)
            {
                centrifuge.SelectGenepacksToCombine(selectedGenepacks[0], selectedGenepacks[1]);
                Close(doCloseSound: false);
            }
            else
            {
                Messages.Message("RS.SelectTwoGenepacks".Translate(), MessageTypeDefOf.RejectInput);
            }
        }

        public override void DrawGenes(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollHeight);
            float curY = 0f;
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
            Rect containingRect = rect2;
            containingRect.y = scrollPosition.y;
            containingRect.height = rect.height;

            DrawGenepackSection(rect, selectedGenepacks, "RS.SelectedGenepacks".Translate() + " (" + selectedGenepacks.Count + "/2)", ref curY, ref selectedHeight, adding: false, containingRect);
            curY += 8f;
            DrawGenepackSection(rect, allGenepacks, "RS.AvailableGenepacks".Translate(), ref curY, ref unselectedHeight, adding: true, containingRect);

            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = curY;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawGenepackSection(Rect rect, List<Genepack> genepacks, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect)
        {
            float curX = 4f;
            Rect rect2 = new Rect(10f, curY, rect.width - 16f - 10f, Text.LineHeight);
            Widgets.Label(rect2, label);
            if (!adding)
            {
                Text.Anchor = TextAnchor.UpperRight;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(rect2, "ClickToAddOrRemove".Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            curY += Text.LineHeight + 3f;
            float num = curY;
            Rect rect3 = new Rect(0f, curY, rect.width, sectionHeight);
            Widgets.DrawRectFast(rect3, Widgets.MenuSectionBGFillColor);
            curY += 4f;
            if (!genepacks.Any())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(rect3, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                for (int i = 0; i < genepacks.Count; i++)
                {
                    Genepack genepack = genepacks[i];
                    if (quickSearchWidget.filter.Active && (!matchingGenepacks.Contains(genepack) || (adding && selectedGenepacks.Contains(genepack))))
                    {
                        continue;
                    }

                    float num2 = 34f + GeneSize.x * (float)genepack.GeneSet.GenesListForReading.Count + 4f * (float)(genepack.GeneSet.GenesListForReading.Count + 2);
                    if (curX + num2 > rect.width - 16f)
                    {
                        curX = 4f;
                        curY += GeneSize.y + 8f + 14f;
                    }
                    if (adding && selectedGenepacks.Contains(genepack))
                    {
                        Widgets.DrawLightHighlight(new Rect(curX, curY, num2, GeneSize.y + 8f));
                        curX += num2 + 14f;
                    }
                    else if (DrawGenepack(genepack, ref curX, curY, num2, containingRect))
                    {
                        if (adding)
                        {
                            if (selectedGenepacks.Count < 2)
                            {
                                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                                selectedGenepacks.Add(genepack);
                            }
                        }
                        else
                        {
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                            selectedGenepacks.Remove(genepack);
                        }
                        OnGenesChanged();
                        break;
                    }
                }
            }
            curY += GeneSize.y + 12f;
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = curY - num;
            }
        }

        private bool DrawGenepack(Genepack genepack, ref float curX, float curY, float packWidth, Rect containingRect)
        {
            bool result = false;
            if (genepack.GeneSet == null || genepack.GeneSet.GenesListForReading.NullOrEmpty())
            {
                return result;
            }
            Rect rect = new Rect(curX, curY, packWidth, GeneSize.y + 8f);
            if (!containingRect.Overlaps(rect))
            {
                curX = rect.xMax + 14f;
                return false;
            }
            Widgets.DrawHighlight(rect);
            GUI.color = OutlineColorUnselected;
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            curX += 4f;
            List<GeneDef> genesListForReading = genepack.GeneSet.GenesListForReading;
            for (int i = 0; i < genesListForReading.Count; i++)
            {
                GeneDef gene = genesListForReading[i];
                if (quickSearchWidget.filter.Active && matchingGenes.Contains(gene))
                {
                    matchingGenepacks.Contains(genepack);
                }
                else
                    _ = 0;
                bool overridden = leftChosenGroups.Any((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(gene));
                Rect geneRect = new Rect(curX, curY + 4f, GeneSize.x, GeneSize.y);
                string extraTooltip = null;
                if (leftChosenGroups.Any((GeneLeftChosenGroup x) => x.leftChosen == gene))
                {
                    extraTooltip = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == gene));
                }
                else if (cachedOverriddenGenes.Contains(gene))
                {
                    extraTooltip = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(gene)));
                }
                else if (randomChosenGroups.ContainsKey(gene))
                {
                    extraTooltip = ("GeneWillBeRandomChosen".Translate() + ":\n" + randomChosenGroups[gene].Select((GeneDef x) => x.label).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
                }
                GeneUIUtility.DrawGeneDef(genesListForReading[i], geneRect, GeneType.Xenogene, () => extraTooltip, doBackground: false, clickable: false, overridden);
                curX += GeneSize.x + 4f;
            }
            Widgets.InfoCardButton(rect.xMax - 24f, rect.y + 2f, genepack);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                result = true;
            }
            curX = Mathf.Max(curX, rect.xMax + 14f);
            return result;
            static string GroupInfo(GeneLeftChosenGroup group)
            {
                if (group == null)
                {
                    return null;
                }
                return ("GeneOneActive".Translate() + ":\n - " + group.leftChosen.LabelCap + " (" + "Active".Translate() + ")" + "\n" + group.overriddenGenes.Select((GeneDef x) => (x.label + " (" + "Suppressed".Translate() + ")").Colorize(ColorLibrary.RedReadable)).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
            }
        }

        public override void UpdateSearchResults()
        {
            quickSearchWidget.noResultsMatched = false;
            matchingGenepacks.Clear();
            matchingGenes.Clear();

            if (!quickSearchWidget.filter.Active)
            {
                return;
            }

            foreach (Genepack genepack in allGenepacks)
            {
                if (quickSearchWidget.filter.Matches(genepack.LabelCap))
                {
                    matchingGenepacks.Add(genepack);
                }
                else
                {
                    foreach (GeneDef gene in genepack.GeneSet.GenesListForReading)
                    {
                        if (quickSearchWidget.filter.Matches(gene.LabelCap))
                        {
                            matchingGenepacks.Add(genepack);
                            matchingGenes.Add(gene);
                            break;
                        }
                    }
                }
            }
            quickSearchWidget.noResultsMatched = !matchingGenepacks.Any();
        }


        public override bool CanAccept()
        {
            if (!base.CanAccept())
            {
                return false;
            }
            if (selectedGenepacks.Count != 2)
            {
                Messages.Message("RS.SelectTwoGenepacks".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }
        
        public override void DoBottomButtons(Rect rect)
        {
            base.DoBottomButtons(rect);
            if (selectedGenepacks.Count == 2)
            {
                int numTicks = centrifuge.CombinationDuration(selectedGenepacks);
                
                Rect rect2 = new Rect(rect.center.x, rect.y, rect.width / 2f - ButSize.x - 10f, ButSize.y);
                TaggedString label = "RS.CombiningDuration".Translate(numTicks.ToStringTicksToPeriod());
                TaggedString taggedString = "RS.CombiningDurationDesc".Translate();
                
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect2, label);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                    TooltipHandler.TipRegion(rect2, taggedString);
                }
            }
        }
        
        public override void DoWindowContents(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMax -= ButSize.y + 4f;
            Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, 35f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect3, Header);
            Text.Font = GameFont.Small;
            DrawSearchRect(rect);
            rect2.yMin += 39f;
            Rect rect4 = new Rect(rect2.x + Margin, rect2.y, rect2.width - Margin * 2f, rect2.height - 8f);
            DrawGenes(rect4);
            
            Rect rect12 = rect;
            rect12.yMin = rect12.yMax - ButSize.y;
            DoBottomButtons(rect12);
        }
    }
}
