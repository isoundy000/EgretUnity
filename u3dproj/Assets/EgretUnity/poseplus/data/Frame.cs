﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FB.PosePlus
{
    ////点操作模式
    //  [Serializable]
    //  public class Dot
    //  {
    //      [SerializeField]
    //      public string name ="noname";
    //      [SerializeField]
    //      public Vector3 position =Vector3.zero;


    //      public static Dot Lerp(Dot left, Dot right, float lerp)
    //      {
    //          Dot m = new Dot();
    //          m.name = right.name;
    //          m.position = Vector3.Lerp(left.position, right.position, lerp);
    //          return m;
    //      }
    //  }
    //  //特效
    //  [Serializable]
    //  public class Effect
    //  {
    //      [SerializeField]
    //      public string name = "noname";
    //      [SerializeField]
    //      public Vector3 position = Vector3.zero;
    //      [SerializeField]
    //      public string follow = "noObj";
    //      [SerializeField]
    //      public int lifeframe = 0;
    //      [SerializeField]
    //      public bool isFollow = false;
    //  }

    [Serializable]
    public class Frame : ICloneable
    {
        [NonSerialized]//每一帧可能有骨骼信息
        public List<string> boneinfo = null;
        [NonSerialized]
        int _bonehash = -1;
        public int bonehash
        {
            set
            {
                _bonehash = value;
            }
            get
            {
                if (_bonehash == -1)
                {
                    string name = "";
                    foreach (var s in boneinfo)
                    {
                        name += s + "|";
                    }
                    _bonehash = name.GetHashCode();
                }
                return _bonehash;
            }

        }


        [SerializeField]
        public int fid;

        [SerializeField]
        public bool key;
        [SerializeField]
        public bool box_key;
        [SerializeField]
        public bool dot_key;
        [SerializeField]
        public float lerp;
        public Frame()
        {

        }
        public Frame(Frame last, int _fid, IList<Transform> trans)
        {
            //Debug.LogWarning("bones=" + trans.Length);

            this.fid = _fid;
            this.key = true;
            //bonesinfo = new PoseBoneMatrix[trans.Count];
            for (int i = 0; i < trans.Count; i++)
            {
                PoseBoneMatrix b = new PoseBoneMatrix();

                bonesinfo.Add(b);
                bonesinfo[i].Record(trans[i], last == null ? null : last.bonesinfo[i]);

            }

        }
        public void LinkLoop(Frame last)
        {
            for (int i = 0; i < bonesinfo.Count; i++)
            {
                bonesinfo[i].Tag(last.bonesinfo[i]);
            }
        }
        [SerializeField]
        public List<PoseBoneMatrix> bonesinfo = new List<PoseBoneMatrix>();
        //[SerializeField]
        //public List<AniBoxCollider> boxesinfo = new List<AniBoxCollider>();
        //[SerializeField]
        //public List<Dot> dotesinfo = new List<Dot>();
        //[SerializeField]
        //public   List<Effect> effectList = new List<Effect>();
        //[SerializeField]
        //public   List<String> aduioList = new List<string>();

        public object Clone()
        {
            Frame fnew = new Frame();
            fnew.fid = this.fid;
            fnew.key = this.key;
            fnew.bonesinfo = new List<PoseBoneMatrix>(bonesinfo);
            return fnew;
        }
        class int3
        {
            public int3(int n, int l, int r)
            {
                idnew = n;
                boneleft = l;
                boneright = r;
            }
            public int idnew;
            public int boneleft;
            public int boneright;
        }
        public static Frame Lerp(Frame left, Frame right, float lerp)
        {
            Frame f = new Frame();
            f.key = false;
            f.fid = left.fid;
            if (left.boneinfo != null && right.boneinfo != null && left.bonehash != right.bonehash)
            {

                Dictionary<string, int3> rebone = new Dictionary<string, int3>();

                for (int i = 0; i < left.boneinfo.Count; i++)
                {
                    string b = left.boneinfo[i];
                    int3 it = new int3(rebone.Count, i, -1);
                    rebone[b] = it;
                }
                for (int i = 0; i < right.boneinfo.Count; i++)
                {
                    string b = right.boneinfo[i];
                    if (rebone.ContainsKey(b) == false)
                    {
                        int3 it = new int3(rebone.Count, -1, i);
                        rebone[b] = it;
                    }
                    else
                    {
                        rebone[b].boneright = i;
                    }
                }
                //Debug.LogWarning("bone mix："+rebone.Count);

                f.boneinfo = new List<string>(rebone.Keys);
                f.bonesinfo = new List<PoseBoneMatrix>(new PoseBoneMatrix[rebone.Count]);

                foreach (var v in rebone.Values)
                {
                    if (v.boneleft == -1)
                        f.bonesinfo[v.idnew] = right.bonesinfo[v.boneright].Clone() as PoseBoneMatrix;
                    else if (v.boneright == -1)
                        f.bonesinfo[v.idnew] = left.bonesinfo[v.boneleft].Clone() as PoseBoneMatrix;
                    else
                    {
                        //Debug.Log("bc:" + v.idnew + "/" + f.bonesinfo.Count);
                        //Debug.Log("lb:" + v.boneleft + "/" + left.bonesinfo.Count);
                        //Debug.Log("rb:" + v.boneright + "/" + right.bonesinfo.Count);
                        f.bonesinfo[v.idnew] = PoseBoneMatrix.Lerp(left.bonesinfo[v.boneleft], right.bonesinfo[v.boneright], lerp);
                    }
                }
            }
            else
            {//single mix
                //f.bonesinfo = new PoseBoneMatrix[left.bonesinfo.Length];
                f.bonesinfo = new List<PoseBoneMatrix>(left.bonesinfo);
                if (left.boneinfo != null)
                {
                    f.boneinfo = left.boneinfo;
                    f.bonehash = left.bonehash;
                }
                for (int i = 0; i < f.bonesinfo.Count; i++)
                {
                    f.bonesinfo[i] = PoseBoneMatrix.Lerp(left.bonesinfo[i], right.bonesinfo[i], lerp);
                }
            }
            return f;

        }

    }
}
