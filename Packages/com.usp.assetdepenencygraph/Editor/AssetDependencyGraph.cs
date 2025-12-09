using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
//using Digit.Client.AssetBundles;
using UnityEngine;
using static Codice.Client.BaseCommands.BranchExplorer.ExplorerData.BrExTreeBuilder.BrExFilter;
//using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Digit.EditorTools.AssetBundles
{
    public static class AssetBundleEditorUtils
    {
        #region Constants
        private const string SearchableTypes = "t:Prefab t:Material t:Shader t:Model t:Scene t:ScriptableObject t:AnimatorController t:AnimationClip t:AnimatorOverrideController t:AudioMixer t:Animation";
        #endregion

        #region Types
        public enum Dependency
        {
            Hard,
            Weak,
        }

        public class Asset
        {
            public string FilePath;

            public string AssetGUID;
        }
        #endregion

        #region Static Fields
        private static HashSet<string> badPathSet = new HashSet<string>();
        #endregion

        #region Static Methods
        [MenuItem("Tools/Asset Dependencies")]
        private static async void X()
        {
            var assetDependencyGraph = await GetAssets();

            var algorithm = CreateAlgorithm(assetDependencyGraph);

            string dotGraphSource = algorithm.Generate();
            Debug.Log(dotGraphSource);

            string outputFilePath = algorithm.Generate(new FileDotEngine(), "AssetDependencyGraph");
            Debug.Log(outputFilePath);
        }

        /*/
        public static IEnumerator<T> FindEverywhere<T>(string searchString, SearchableEditorWindow.SearchMode mode, Func<HierarchyIterator, T> selector)
        {
            List<string> list = new List<string>();
            if (searchFilter.searchArea == SearchFilter.SearchArea.AllAssets || searchFilter.searchArea == SearchFilter.SearchArea.InAssetsOnly)
            {
                list.Add("Assets");
            }

            if (searchFilter.searchArea == SearchFilter.SearchArea.AllAssets || searchFilter.searchArea == SearchFilter.SearchArea.InPackagesOnly)
            {
                UnityEditor.PackageManager.PackageInfo[] allVisiblePackages = PackageManagerUtilityInternal.GetAllVisiblePackages(searchFilter.skipHidden);
                foreach (UnityEditor.PackageManager.PackageInfo packageInfo in allVisiblePackages)
                {
                    list.Add(packageInfo.assetPath);
                }
            }

            HierarchyIterator lastProperty = null;
            foreach (string item in list)
            {
                HierarchyIterator property = new HierarchyIterator(item);
                if (lastProperty != null)
                {
                    property.CopySearchFilterFrom(lastProperty);
                }
                else
                {
                    property.SetSearchFilter(searchFilter);
                }

                lastProperty = property;
                while (property.Next(null))
                {
                    yield return selector(property);
                }
            }
        }
        //*/

        //*/
        public static async IAsyncEnumerable<string> GetAssetGUIDs(string searchableTypes = SearchableTypes)
        {
            string[] assetGuids = AssetDatabase.FindAssets(searchableTypes);
            int assetLength = assetGuids.Length;

            foreach (string assetGuid in assetGuids)
            {
                await Awaitable.BackgroundThreadAsync();

                yield return assetGuid;
            }
        }

        public static async Awaitable<AdjacencyGraph<Asset, STaggedEdge<Asset, Dependency>>> GetAssets(
            string searchableTypes = SearchableTypes,
            bool allowScriptReferences = false)
        {
            var assetDependencyGraph = new AdjacencyGraph<Asset, STaggedEdge<Asset, Dependency>>(false);

            IAsyncEnumerable<string> guidsFilteredAssets = GetAssetGUIDs(searchableTypes);

            await foreach (string assetGuid in guidsFilteredAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                AddAssetNode(assetDependencyGraph, assetGuid, assetPath, allowScriptReferences);
            }

            return assetDependencyGraph;
        }

        private static Asset AddAssetNode(AdjacencyGraph<Asset, STaggedEdge<Asset, Dependency>> assetDependencyGraph, string assetGuid, string assetPath, bool allowScriptReferences)
        {
            if (!allowScriptReferences &&
                     (assetPath.EndsWith(".cs", System.StringComparison.InvariantCultureIgnoreCase) ||
                       assetPath.EndsWith(".js", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                return default;
            }

            var asset = new Asset()
            {
                AssetGUID = assetGuid,
                FilePath = assetPath,
            };

            if (assetDependencyGraph.ContainsVertex(asset))
            {
                // The asset has been visited.
                return default;
            }

            assetDependencyGraph.AddVertex(asset);

            string[] dependencyAssetPaths = AssetDatabase.GetDependencies(assetPath, false);

            foreach (string dependencyAssetPath in dependencyAssetPaths)
            {
                string dependencyAssetGuid = AssetDatabase.AssetPathToGUID(dependencyAssetPath);

                // TODO: Add a eay to transform one asset into many. e.g. Variants

                var dependency = AddAssetNode(assetDependencyGraph, dependencyAssetGuid, dependencyAssetPath, allowScriptReferences);

                if (dependency == default)
                {
                    continue;
                }

                var edge = new STaggedEdge<Asset, Dependency>(asset, dependency, Dependency.Hard);
                assetDependencyGraph.AddEdge(edge);
            }

            // TODO: Add custom dependency types. e.g. Soft dependencies

            return asset;
        }

        private static GraphvizAlgorithm<Asset, STaggedEdge<Asset, Dependency>> CreateAlgorithm(AdjacencyGraph<Asset, STaggedEdge<Asset, Dependency>> assetDependencyGraph)
        {
            var graphvizAlgorithm = new GraphvizAlgorithm<Asset, STaggedEdge<Asset, Dependency>>(assetDependencyGraph);
            graphvizAlgorithm.CommonVertexFormat.Shape = GraphvizVertexShape.Rectangle;
            graphvizAlgorithm.CommonEdgeFormat.ToolTip = "Edge tooltip";
            graphvizAlgorithm.FormatVertex += (sender, args) =>
            {
                args.VertexFormat.IsHtmlLabel = true;
                args.VertexFormat.Label = $"<table border=\"0\" cellborder=\"0\" cellspacing=\"0\">" +
                $"<tr><td><b>{args.Vertex.FilePath}</b></td></tr>" +
                $"<tr><td>GUID: {args.Vertex.AssetGUID}</td></tr>" +
                $"<tr><td>Main Asset Type: {AssetDatabase.GetMainAssetTypeAtPath(args.Vertex.FilePath)}</td></tr>" +
                $"</table>";
            };
            graphvizAlgorithm.FormatEdge += (sender, args) =>
            {
                args.EdgeFormat.Style = GraphvizEdgeStyle.Solid;
            };

            return graphvizAlgorithm;
        }
        #endregion
    }
}
