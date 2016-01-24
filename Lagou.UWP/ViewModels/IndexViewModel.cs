﻿using Caliburn.Micro;
using Lagou.API;
using Lagou.API.Methods;
using Lagou.UWP.Attributes;
using Lagou.UWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lagou.UWP.ViewModels {

    [Regist(InstanceMode.Singleton)]
    public class IndexViewModel : BasePageVM {
        public override char Glyph {
            get {
                return (char)0xf015;// "&#xf015;";
            }
        }

        public override string Title {
            get {
                return "职位列表";
            }
        }

        public BindableCollection<SearchedItemViewModel> Datas { get; set; }


        public SearchBarViewModel SearchBarVM { get; set; }


        private int Page = 1;

        private INavigationService NS = null;

        public ICommand LoadMoreCmd { get; set; }

        public ICommand ReloadCmd { get; set; }


        public IndexViewModel(SimpleContainer container, INavigationService ns) {
            this.NS = ns;

            this.SearchBarVM = container.GetInstance<SearchBarViewModel>();

            this.ReloadCmd = new Command(async () => {
                await this.LoadData(true);
            });

            this.LoadMoreCmd = new Command(async () => {
                await this.LoadData(false);
            });
        }


        protected override void OnActivate() {
            base.OnActivate();

            if (this.Datas == null) {
                this.Datas = new BindableCollection<SearchedItemViewModel>();
                Task.Delay(500).ContinueWith(async t => {
                    await this.LoadData();
                });
            }
        }

        private async Task LoadData(bool reload = false) {
            this.IsBusy = true;

            var method = new Search() {
                Page = reload ? 1 : this.Page
            };
            var datas = await ApiClient.Execute(method);
            if (!method.HasError && datas.Count() > 0) {

                if (reload) {
                    this.Datas.Clear();
                }


                this.Datas.AddRange(datas.Select(d =>
                    new SearchedItemViewModel(d, this.NS)
                ));

                this.Page++;
            }

            this.IsBusy = false;
        }
    }
}
