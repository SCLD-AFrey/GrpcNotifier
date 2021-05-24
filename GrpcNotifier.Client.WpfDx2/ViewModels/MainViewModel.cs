using System;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Google.Protobuf.WellKnownTypes;
using GrpcNotifier.Common;
using GrpcNotifier.Data;


namespace GrpcNotifier.Client.WpfDxAlt.ViewModels
{
    [MetadataType(typeof(MetaData))]
    public class MainViewModel
    {
        public class MetaData : IMetadataProvider<MainViewModel>
        {
            void IMetadataProvider<MainViewModel>.BuildMetadata
                (MetadataBuilder<MainViewModel> p_builder)
            {
                p_builder.CommandFromMethod(p_x => p_x.OnLockPersonScriptCommand()).CommandName("LockPersonScriptCommand");
                p_builder.CommandFromMethod(p_x => p_x.OnUpdatePersonScriptCommand()).CommandName("UpdatePersonScriptCommand");
                p_builder.Property(p_x => p_x.SelectedPerson).OnPropertyChangedCall(p_x => p_x.OnSelectedPersonChanged());
            }
        }

        #region Constructors

        protected MainViewModel()
        {
            unitOfWork = new UnitOfWork()
            {
                ConnectionString = "XpoProvider=MSSqlServer;data source=(localdb)\\MSSQLLocalDB;integrated security=SSPI;initial catalog=SampleData",
                AutoCreateOption = AutoCreateOption.DatabaseAndSchema
            };
            PersonCollection = new XPCollection<Person>(unitOfWork);
        }

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        #endregion

        #region Fields and Properties

        public virtual UnitOfWork unitOfWork { get; set; }
        public virtual XPCollection<Person> PersonCollection { get; set; }
        public virtual Person SelectedPerson { get; set; }
        public virtual bool IsRecordLocked { get; set; }
        private static readonly NotificationServiceClient m_notificationService = new();
        private static string m_originId = Guid.NewGuid().ToString();

        #endregion

        #region Methods


        public async void WriteCommandExecute(string content)
        {
            await m_notificationService.Write(new NotificationLog
            {
                OriginId = m_originId,
                Content = content,
                At = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            });
        }
        
        public void OnUpdatePersonScriptCommand()
        {
            
        }
        public void OnLockPersonScriptCommand()
        {
            SelectedPerson.IsLocked = !SelectedPerson.IsLocked;
            unitOfWork.CommitChanges();
            WriteCommandExecute($"Person [{SelectedPerson.Oid}] has been {((SelectedPerson.IsLocked) ? "Locked" : "Unlocked")}");
            IsRecordLocked = SelectedPerson.IsLocked;
        }

        public void OnSelectedPersonChanged()
        {
            IsRecordLocked = SelectedPerson.IsLocked;
        }
        
        #endregion
    }
}