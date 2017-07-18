using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.CropImage;

namespace CropSample
{
    public partial class ViewController : UIViewController, ITOCropViewControllerDelegate
    {
        ImagePickerDelegate imagePickerDelegate;
        CropViewControllerDelegate cropControllerDelegate;
        UIImage image;
        UIImageView imageView;
        CGRect croppedFrame;
        TOCropViewCroppingStyle croppingStyle;
        int angle;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            imagePickerDelegate = new ImagePickerDelegate(this);
            cropControllerDelegate = new CropViewControllerDelegate(this);

            this.Title = "ToCropViewController";
            this.NavigationController.NavigationBar.Translucent = false;

            this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, ShowCropViewController);
            this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, SharePhoto);
            this.NavigationItem.RightBarButtonItem.Enabled = false;

            this.imageView = new UIImageView();
            this.imageView.UserInteractionEnabled = true;
            this.imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            Add(imageView);

            var tapGestureRecognizer = new UITapGestureRecognizer(DidTapImageView);
            this.View.AddGestureRecognizer(tapGestureRecognizer);
        }

        void ShowCropViewController(object sender, EventArgs e)
        {
            var alertController = new UIAlertController();

            var defaultAction = UIAlertAction.Create("Crop Image", UIAlertActionStyle.Default, (obj) =>
            {
                this.croppingStyle = TOCropViewCroppingStyle.Default;

                var standardPicker = new UIImagePickerController();
                standardPicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                standardPicker.AllowsEditing = false;
                standardPicker.Delegate = imagePickerDelegate;
                this.PresentViewController(standardPicker, true, null);
            });

            var profileAction = UIAlertAction.Create("Make Profile Picture", UIAlertActionStyle.Default, (obj) =>
            {
                this.croppingStyle = TOCropViewCroppingStyle.Circular;

                var profilePicker = new UIImagePickerController();
                profilePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                profilePicker.AllowsEditing = false;
                profilePicker.Delegate = imagePickerDelegate;
                profilePicker.PreferredContentSize = new CGSize(512, 512);
                //profilePicker.PopoverPresentationController.BarButtonItem = this.NavigationItem.LeftBarButtonItem;
                this.PresentViewController(profilePicker, true, null);
            });

            alertController.AddAction(defaultAction);
            alertController.AddAction(profileAction);
            alertController.ModalPresentationStyle = UIModalPresentationStyle.Popover;

            //var popPresenter = alertController.PopoverPresentationController;
            //popPresenter.BarButtonItem = this.NavigationItem.LeftBarButtonItem;
            this.PresentViewController(alertController, true, null);
        }

        void SharePhoto(object sender, EventArgs e)
        {
            if (imageView.Image == null)
                return;

            var activityController = new UIActivityViewController(new NSObject[] { imageView.Image }, null);
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                PresentViewController(activityController, true, null);
            }
        }

        void LayoutImageView()
        {
            if (imageView.Image == null)
                return;

            var padding = 20f;
            var viewFrame = View.Bounds;
            viewFrame.Inflate(-padding * 2, -padding * 2);

            var imageFrame = new CGRect();
            imageFrame.Size = imageView.Image.Size;

            if (imageView.Image.Size.Width > viewFrame.Width ||
                 imageView.Image.Size.Height > viewFrame.Height)
            {
                var scale = Math.Min(viewFrame.Width / imageFrame.Width, viewFrame.Height / imageFrame.Height);
                imageFrame.Width *= (nfloat)scale;
                imageFrame.Height *= (nfloat)scale;
                imageFrame.X = View.Bounds.Width / 2 - imageFrame.Width / 2;
                imageFrame.Y = View.Bounds.Height / 2 - imageFrame.Height / 2;
                imageView.Frame = imageFrame;
            }
            else
            {
                imageView.Frame = imageFrame;
                imageView.Center = new CGPoint(View.Bounds.GetMidX(), View.Bounds.GetMidY());
            }
        }

        void DidTapImageView()
        {
            if (image == null)
                return;

            var cropController = new TOCropViewController(this.croppingStyle, this.image);
            cropController.Delegate = this.cropControllerDelegate;
            var viewFrame = this.View.ConvertRectToView(imageView.Frame, NavigationController.View);
            cropController.PresentAnimatedFromParentViewController(
                this,
                imageView.Image,
                null,
                viewFrame,
                this.angle,
                this.croppedFrame,
                () => imageView.Hidden = true,
                null
            );
        }

        void UpdateImageViewWithImage(UIImage croppedImage, TOCropViewController cropViewController)
        {
            imageView.Image = croppedImage;
            LayoutImageView();

            this.NavigationItem.RightBarButtonItem.Enabled = true;

            if (cropViewController.CroppingStyle == TOCropViewCroppingStyle.Circular)
            {
                this.imageView.Hidden = true;
                cropViewController.DismissAnimatedFromParentViewController(
                    this,
                    croppedImage,
                    imageView,
                    CGRect.Empty,
                    LayoutImageView,
                    () => imageView.Hidden = false
                );
            }

            else
            {
                imageView.Hidden = false;
                cropViewController.PresentingViewController.DismissViewController(true, null);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #region Image Picker Delegate
        class ImagePickerDelegate : UIImagePickerControllerDelegate
        {
            ViewController viewController;

            public ImagePickerDelegate(ViewController view)
            {
                viewController = view;
            }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                viewController.image = (UIImage)info.ObjectForKey(UIImagePickerController.OriginalImage);

                var cropController = new TOCropViewController(viewController.croppingStyle, viewController.image);
                cropController.Delegate = viewController.cropControllerDelegate;

                //If profile picture, push onto the same navigation stack
                if (viewController.croppingStyle == TOCropViewCroppingStyle.Circular)
                    picker.PushViewController(cropController, true);

                //otherwise dismiss, and then present from the main controller
                else
                    picker.DismissViewController(true, () => viewController.PresentViewController(cropController, true, null));
            }

            public override void Canceled(UIImagePickerController picker)
            {
                viewController.DismissViewController(true, null);
            }
        }
        #endregion

        #region Crop View Controller Delegate
        class CropViewControllerDelegate : TOCropViewControllerDelegate
        {
            ViewController viewController;

            public CropViewControllerDelegate(ViewController view)
            {
                this.viewController = view;
            }

            public override void DidCropToImage(TOCropViewController cropViewController, UIImage image, CGRect cropRect, nint angle)
            {
                viewController.croppedFrame = cropRect;
                viewController.angle = (int)angle;
                viewController.UpdateImageViewWithImage(image, cropViewController);
            }

            public override void DidCropToCircularImage(TOCropViewController cropViewController, UIImage image, CGRect cropRect, nint angle)
            {
                viewController.croppedFrame = cropRect;
                viewController.angle = (int)angle;
                viewController.UpdateImageViewWithImage(image, cropViewController);
            }

            public override void DidFinishCancelled(TOCropViewController cropViewController, bool cancelled)
            {
                viewController.DismissViewController(true, null);
            }
        }
        #endregion
    }
}
