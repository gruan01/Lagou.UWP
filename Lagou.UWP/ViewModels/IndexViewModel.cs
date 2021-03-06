﻿using Caliburn.Micro;
using Lagou.API;
using Lagou.API.Methods;
using Lagou.UWP.Attributes;
using Lagou.UWP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        private INavigationService NS = null;

        public ICommand LoadMoreCmd { get; set; }

        public ICommand ReloadCmd { get; set; }


        #region used for ListView Position Restore
        public Func<object, string> KeyFinder { get; set; }
            = o => {
                var item = (SearchedItemViewModel)o;
                if (item != null) {
                    return item.Data.PositionId.ToString();
                }
                return string.Empty;
            };

        public string PersistedItemKey { get; set; }
        #endregion

        private string _city = "";
        private string _keyword = "";
        private int _page = 1;

        public IndexViewModel(SimpleContainer container, INavigationService ns) {
            this.NS = ns;

            this.SearchBarVM = container.GetInstance<SearchBarViewModel>();
            this.SearchBarVM.OnSubmit += SearchBarVM_OnSubmit;

            this.ReloadCmd = new Command(async () => {
                await this.LoadData(true);
            });

            this.LoadMoreCmd = new Command(async () => {
                await this.LoadData(false);
            });
        }

        private async void SearchBarVM_OnSubmit(object sender, SearchBarEventArgs e) {
            this._city = e.City;
            this._keyword = e.Keyword;

            await this.LoadData(true);
        }

        protected override void OnActivate() {
            base.OnActivate();

            if (this.Datas == null) {
                Task.Delay(500).ContinueWith(async t => {
                    await this.LoadData();
                });
            }
        }

        public async Task ReloadData() {
            await this.LoadData(true);
        }

        public async Task LoadData(bool reload = false) {
            if (this.Datas == null) {
                this.Datas = new BindableCollection<SearchedItemViewModel>();
                this.NotifyOfPropertyChange(() => this.Datas);
            }

            this.IsBusy = true;

            var method = new Search() {
                Page = reload ? 1 : this._page,
                Key = this._keyword,
                City = this._city
            };
            var datas = await ApiClient.Execute(method);
            if (!method.HasError && datas.Count() > 0) {

                if (reload) {
                    this.Datas.Clear();
                }


                this.Datas.AddRange(datas.Select(d =>
                    new SearchedItemViewModel(d, this.NS)
                ));

                //this._page++;
                this._page = reload ? 2 : this._page++;
            }

            this.IsBusy = false;
        }


        // OnViewAttached -> OnViewReady -> OnViewLoaded

        //protected override void OnViewAttached(object view, object context) {
        //    base.OnViewAttached(view, context);
        //}

        //protected override void OnViewReady(object view) {
        //    base.OnViewReady(view);
        //}

        //protected override void OnViewLoaded(object view) {
        //    base.OnViewLoaded(view);
        //}
    }
}
