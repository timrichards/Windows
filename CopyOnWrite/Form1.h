#pragma once

namespace CopyOnWrite {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::IO; // Directory
	using namespace System::Windows::Forms; // DialogResult

	/// <summary>
	/// Summary for Form1
	/// </summary>
	public ref class Form1 : public System::Windows::Forms::Form
	{
	public:
		Form1(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
				 this->Icon = gcnew System::Drawing::Icon( SystemIcons::Shield,40,40 );
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~Form1()
		{
			if (components)
			{
				delete components;
			}
		}

	private:
		bool bIconAction;
		bool bIconSwitch;
		String^ strTitle;

	private: System::IO::FileSystemWatcher^  fileSystemWatcher1;
	protected: 
	private: System::Windows::Forms::Button^  button1;
	private: System::Windows::Forms::Button^  button_FromFolder;
	private: System::Windows::Forms::Label^  label_ToFolder;
	private: System::Windows::Forms::Label^  label_FromFolder;
	private: System::Windows::Forms::FolderBrowserDialog^  folderBrowserDialog1;
	private: System::Windows::Forms::Timer^  timer_ChangeIcon;
	private: System::ComponentModel::IContainer^  components;
	private: System::Windows::Forms::Label^  label_FlashesIfNotFrozen;
	private: System::Windows::Forms::CheckBox^  checkBox_Disable;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


	private: System::Windows::Forms::Timer^  timer_ChangeTitle;



#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			this->fileSystemWatcher1 = (gcnew System::IO::FileSystemWatcher());
			this->folderBrowserDialog1 = (gcnew System::Windows::Forms::FolderBrowserDialog());
			this->label_ToFolder = (gcnew System::Windows::Forms::Label());
			this->button_FromFolder = (gcnew System::Windows::Forms::Button());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->timer_ChangeIcon = (gcnew System::Windows::Forms::Timer(this->components));
			this->timer_ChangeTitle = (gcnew System::Windows::Forms::Timer(this->components));
			this->label_FlashesIfNotFrozen = (gcnew System::Windows::Forms::Label());
			this->checkBox_Disable = (gcnew System::Windows::Forms::CheckBox());
			this->label_FromFolder = (gcnew System::Windows::Forms::Label());
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->fileSystemWatcher1))->BeginInit();
			this->SuspendLayout();
			// 
			// fileSystemWatcher1
			// 
			this->fileSystemWatcher1->EnableRaisingEvents = true;
			this->fileSystemWatcher1->IncludeSubdirectories = true;
			this->fileSystemWatcher1->NotifyFilter = System::IO::NotifyFilters::LastWrite;
			this->fileSystemWatcher1->SynchronizingObject = this;
			// 
			// folderBrowserDialog1
			// 
			this->folderBrowserDialog1->RootFolder = System::Environment::SpecialFolder::DesktopDirectory;
			this->folderBrowserDialog1->ShowNewFolderButton = false;
			// 
			// label_ToFolder
			// 
			this->label_ToFolder->AutoSize = true;
			this->label_ToFolder->Font = (gcnew System::Drawing::Font(L"Tahoma", 9.75F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(0)));
			this->label_ToFolder->Location = System::Drawing::Point(12, 97);
			this->label_ToFolder->Name = L"label_ToFolder";
			this->label_ToFolder->Size = System::Drawing::Size(63, 16);
			this->label_ToFolder->TabIndex = 3;
			this->label_ToFolder->Text = L"To Folder";
			// 
			// button_FromFolder
			// 
			this->button_FromFolder->Location = System::Drawing::Point(12, 12);
			this->button_FromFolder->Name = L"button_FromFolder";
			this->button_FromFolder->Size = System::Drawing::Size(75, 23);
			this->button_FromFolder->TabIndex = 4;
			this->button_FromFolder->Text = L"From Folder";
			this->button_FromFolder->UseVisualStyleBackColor = true;
			this->button_FromFolder->Click += gcnew System::EventHandler(this, &Form1::button_FromFolder_Click);
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(15, 71);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(75, 23);
			this->button1->TabIndex = 5;
			this->button1->Text = L"To Folder";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &Form1::button1_Click);
			// 
			// timer_ChangeIcon
			// 
			this->timer_ChangeIcon->Interval = 15000;
			this->timer_ChangeIcon->Tick += gcnew System::EventHandler(this, &Form1::timer_ChangeIcon_Tick);
			// 
			// timer_ChangeTitle
			// 
			this->timer_ChangeTitle->Interval = 1000;
			this->timer_ChangeTitle->Tick += gcnew System::EventHandler(this, &Form1::timer_ChangeTitle_Tick);
			// 
			// label_FlashesIfNotFrozen
			// 
			this->label_FlashesIfNotFrozen->AutoSize = true;
			this->label_FlashesIfNotFrozen->Font = (gcnew System::Drawing::Font(L"Tahoma", 9.75F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(0)));
			this->label_FlashesIfNotFrozen->Location = System::Drawing::Point(194, 137);
			this->label_FlashesIfNotFrozen->Name = L"label_FlashesIfNotFrozen";
			this->label_FlashesIfNotFrozen->Size = System::Drawing::Size(78, 16);
			this->label_FlashesIfNotFrozen->TabIndex = 6;
			this->label_FlashesIfNotFrozen->Text = L"[Monitoring]";
			this->label_FlashesIfNotFrozen->TextAlign = System::Drawing::ContentAlignment::BottomRight;
			// 
			// checkBox_Disable
			// 
			this->checkBox_Disable->AutoSize = true;
			this->checkBox_Disable->Checked = true;
			this->checkBox_Disable->CheckState = System::Windows::Forms::CheckState::Checked;
			this->checkBox_Disable->Location = System::Drawing::Point(15, 137);
			this->checkBox_Disable->Name = L"checkBox_Disable";
			this->checkBox_Disable->Size = System::Drawing::Size(61, 17);
			this->checkBox_Disable->TabIndex = 7;
			this->checkBox_Disable->Text = L"Disable";
			this->checkBox_Disable->UseVisualStyleBackColor = true;
			// 
			// label_FromFolder
			// 
			this->label_FromFolder->AutoSize = true;
			this->label_FromFolder->Font = (gcnew System::Drawing::Font(L"Tahoma", 9.75F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(0)));
			this->label_FromFolder->Location = System::Drawing::Point(12, 38);
			this->label_FromFolder->Name = L"label_FromFolder";
			this->label_FromFolder->Size = System::Drawing::Size(78, 16);
			this->label_FromFolder->TabIndex = 2;
			this->label_FromFolder->Text = L"From Folder";
			// 
			// Form1
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(284, 162);
			this->Controls->Add(this->checkBox_Disable);
			this->Controls->Add(this->label_FlashesIfNotFrozen);
			this->Controls->Add(this->button1);
			this->Controls->Add(this->button_FromFolder);
			this->Controls->Add(this->label_ToFolder);
			this->Controls->Add(this->label_FromFolder);
			this->Name = L"Form1";
			this->Text = L"CopyOnWrite";
			this->Load += gcnew System::EventHandler(this, &Form1::Form1_Load);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->fileSystemWatcher1))->EndInit();
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void timer_ChangeIcon_Tick(System::Object^  sender, System::EventArgs^  e) {
				 if (bIconSwitch || (checkBox_Disable->CheckState == System::Windows::Forms::CheckState::Checked))
				 {
					this->Icon = gcnew System::Drawing::Icon( SystemIcons::Shield,40,40 );
				 }
				 else if (bIconAction)
				 {
					this->Icon = gcnew System::Drawing::Icon( SystemIcons::Information,40,40 );
					bIconAction = false;
				 }
				 else
				 {
					 this->Icon = gcnew System::Drawing::Icon( SystemIcons::Warning,40,40 );
				 }

				 bIconSwitch = !bIconSwitch;
			 }
private: System::Void Form1_Load(System::Object^  sender, System::EventArgs^  e) {
			 strTitle = label_FlashesIfNotFrozen->Text;
			 timer_ChangeIcon->Start();
			 timer_ChangeTitle->Start();
		 }
private: System::Void timer_ChangeTitle_Tick(System::Object^  sender, System::EventArgs^  e) {
			 if ((label_FlashesIfNotFrozen->Text->Length == 0) &&
				 (checkBox_Disable->CheckState == System::Windows::Forms::CheckState::Unchecked))
			 {
				 label_FlashesIfNotFrozen->Text = strTitle;
			 }
			 else
			 {
				 label_FlashesIfNotFrozen->Text = "";
			 }
		 }
private: System::Void button_FromFolder_Click(System::Object^  sender, System::EventArgs^  e) {
			 folderBrowserDialog1->ShowNewFolderButton = false;
			 if (Directory::Exists(label_FromFolder->Text))
			 {
				 folderBrowserDialog1->SelectedPath = label_FromFolder->Text;
			 }
			 else
			 {
				 folderBrowserDialog1->SelectedPath = "";
			 }

			  System::Windows::Forms::DialogResult result = folderBrowserDialog1->ShowDialog();

			  if ( result == System::Windows::Forms::DialogResult::OK )
			  {
				  /*::app::Default->FromFolder =*/ label_FromFolder->Text = folderBrowserDialog1->SelectedPath;
			  }
		 }
private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) {
			 folderBrowserDialog1->ShowNewFolderButton = false;
			 if (Directory::Exists(label_ToFolder->Text))
			 {
				 folderBrowserDialog1->SelectedPath = label_ToFolder->Text;
			 }
			 else
			 {
				 folderBrowserDialog1->SelectedPath = "";
			 }

			  System::Windows::Forms::DialogResult result = folderBrowserDialog1->ShowDialog();

			  if ( result == System::Windows::Forms::DialogResult::OK )
			  {
				  /*::app::Default->ToFolder =*/ label_ToFolder->Text = folderBrowserDialog1->SelectedPath;
			  }
		 }
};
}

