using FileWalkerImmutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileWalkerImmutableTest
{
    [TestClass]
    public class FileSystemFacadeTest
    {
        FileSystemFacade fs;

        [TestInitialize]
        public void Init()
        {
            fs = new FileSystemFacade();
        }

        [TestMethod]
        public void TestCreateFolder_CreatesFolderWithCorrectName()
        {
            // Arrange
            IComponent folder = fs.CreateFolder("folder");
            IComponent file = fs.CreateFile("file1", 100, "The context");

            // Act
            fs.AddChildren(folder, file);

            // Assert
            Assert.AreEqual("folder", folder.Name);
            Assert.AreEqual("file1", file.Name);
            Assert.IsInstanceOfType(folder, typeof(IFolder));
            Assert.IsInstanceOfType(file, typeof(IFile));
        }


        [TestMethod]
        public void TestGetFileOrDirectory()
        {
            // Arrange
            IComponent root = fs.CreateFolder("Root");
            IComponent folder1 = fs.CreateFolder("Folder1");
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of file1.");
            fs.AddChildren(folder1, file1);
            fs.AddChildren(root, folder1);

            // Act
            string[] path = new string[] { "Folder1", "File1"};
            IComponent file = fs.GetComponentByPath(root, path);
            
            string invalidPath = "File2";
            IComponent invalidFile = fs.GetComponentByPath(root, invalidPath);

            // Assert
            Assert.AreEqual(file, file1);
            Assert.IsNull(invalidFile);
        }

        [TestMethod]
        public void TestGetFileOrDirectoryNotValide()
        {
            // Arrange
            IComponent root = fs.CreateFolder("Root");
            IComponent folder1 = fs.CreateFolder("Folder1");
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of file1.");
            fs.AddChildren(folder1, file1);
            fs.AddChildren(root, folder1);

            // Act
            string[] path = new string[] { "Folder1", "File3" };
            IComponent file = fs.GetComponentByPath(root, path);

            Assert.IsNull(file);
        }

        [TestMethod]
        public void TestGetFileOrDirectoryWithoutPath()
        {
            // Arrange
            IComponent folder1 = fs.CreateFolder("Folder1");
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of file1.");
            fs.AddChildren(folder1, file1);

            // Act
            string[] path = new string[] { };
            IComponent file = fs.GetComponentByPath(folder1, path);

            Assert.AreEqual(folder1.Name, file.Name);
        }

        [TestMethod]
        public void TestRename()
        {
            // Arrange
            IComponent folder = fs.CreateFolder("folder");
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of file1.");
            fs.AddChildren(folder, file1);

            // Act
            fs.Rename(file1, "File1-new");

            // Assert
            IComponent oldFile = fs.GetComponentByPath(folder, "File1");
            Assert.IsNull(oldFile);

            IFile fileRenamed = fs.GetComponentByPath(folder, "File1-new") as IFile;
            Assert.AreEqual(fileRenamed.Name, "File1-new");
            Assert.AreEqual(fileRenamed.Size, 100);
            Assert.AreEqual(fileRenamed.Content, "This is the content of file1.");
        }

        [TestMethod]
        public void TestRenameNotExistingFile()
        {
            // Arrange
            IComponent file1 = null;

            // Act
            Assert.ThrowsException<NullReferenceException>(() => fs.Rename(file1, "new-file"));
        }

        [TestMethod]
        public void TestRemoveSimpleFile()
        {
            IComponent root = fs.CreateFolder("Root");
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of File1.");
            fs.AddChildren(root, file1);

            // Act
            fs.Delete(file1);

            // Assert
            IComponent oldFile1 = fs.GetComponentByPath(root, "File1");
            Assert.IsNull(oldFile1);
        }

        [TestMethod]
        public void TestRemoveSimpleFolder()
        {
            IComponent root = fs.CreateFolder("Root");
            IComponent folder = fs.CreateFolder("Root");
            fs.AddChildren(root, folder);

            // Act
            fs.Delete(folder);

            // Assert
            IComponent oldFolder= fs.GetComponentByPath(root, folder.Name);
            Assert.IsNull(oldFolder);
        }

        [TestMethod]
        public void TestRemoveFolderContainsFiles()
        {
            IComponent root = fs.CreateFolder("root");
            IComponent folder = fs.CreateFolder("folder");
            IComponent file = fs.CreateFile("File1", 100, "This is the content of File1.");
            fs.AddChildren(folder, file);
            fs.AddChildren(root, folder);

            // Act
            fs.Delete(folder);

            // Assert
            IComponent oldFolder= fs.GetComponentByPath(root, folder.Name);
            Assert.IsNull(oldFolder);

            IComponent oldFile = fs.GetComponentByPath(root, file.Name);
            Assert.ThrowsException<NullReferenceException>(() => {
                // folder.Name is null
                string[] path = new string[] { folder.Name, file.Name };
                IComponent oldFile = fs.GetComponentByPath(root, path);
                Assert.IsNull(oldFile);
            });
        }

        [TestMethod]
        public void TestRemoveNotExistsFile()
        {
            IComponent root = fs.CreateFolder("Root");
            IComponent file = fs.CreateFile("File1", 100, "This is the content of File1.");
            fs.AddChildren(root, file);

            IComponent file2 = fs.GetComponentByPath(root, "file2");

            Assert.ThrowsException<NullReferenceException>(() => fs.Delete(file2));
        }


        [TestMethod]
        public void TestRemoveNotExistsFolder()
        {
            IComponent root = fs.CreateFolder("Root");
            IComponent folder = fs.CreateFolder("Folder");
            fs.AddChildren(root, folder);

            IComponent folder2 = fs.GetComponentByPath(root, "folder2");

            Assert.ThrowsException<NullReferenceException>(() => fs.Delete(folder2));
        }


        [TestMethod]
        public void TestNotificationLogEmpty()
        {
            Assert.IsTrue(fs.NotificationLog.Count == 0);
        }

        [TestMethod]
        public void TestNotificationIsAdded()
        {
            // Arrange
            IComponent file1 = fs.CreateFile("File1", 100, "This is the content of File1.");
            fs.NotifyOnChange(file1);

            // Act
            fs.Rename(file1, "file2");

            // Assert
            Assert.IsTrue(fs.NotificationLog.Count == 1);
            Assert.IsTrue(fs.NotificationLog.Pop() == $"{file1.Name} was renamed to file2.");
        }

        [TestMethod]
        public void TestNotNotifyWhenUnSelectedFileOrFolder()
        {
            // Arrange
            IComponent root = fs.CreateFolder("root");
            IComponent folderSelected = fs.CreateFolder("folderSelected");
            IComponent folderUnSelected = fs.CreateFolder("folderUnSelected");
            IComponent selectedFile = fs.CreateFile("selectedFile", 100, "This is the content of selectedFile.");
            IComponent unSelectedFile = fs.CreateFile("unSelectedFile", 200, "This is the content of unSelectedFile.");

            fs.AddChildren(root, folderSelected, folderUnSelected, selectedFile, unSelectedFile);
            fs.NotifyOnChange(folderSelected);
            fs.NotifyOnChange(selectedFile);

            // Act
            fs.Rename(folderSelected, "folderSelected-renamed");
            fs.Rename(selectedFile, "selectedFile-renamed");

            // Assert
            Assert.IsTrue(fs.NotificationLog.Count == 2);
            Assert.IsTrue(fs.NotificationLog.Contains("folderSelected was renamed to folderSelected-renamed."));
            Assert.IsTrue(fs.NotificationLog.Contains("selectedFile was renamed to selectedFile-renamed."));
            Assert.IsFalse(fs.NotificationLog.Contains("folderUnSelected was renamed to folderUnSelected-renamed."));
            Assert.IsFalse(fs.NotificationLog.Contains("unSelectedFile was renamed to unSelectedFile-renamed."));
        }
    }
}
