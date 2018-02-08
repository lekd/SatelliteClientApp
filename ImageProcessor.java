package com.example.lkduy.panomaraviewermockup;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.util.Log;

import org.opencv.android.Utils;
import org.opencv.calib3d.Calib3d;
import org.opencv.core.Core;
import org.opencv.core.CvException;
import org.opencv.core.CvType;
import org.opencv.core.Mat;
import org.opencv.core.MatOfPoint;
import org.opencv.core.MatOfPoint2f;
import org.opencv.core.Point;
import org.opencv.core.Scalar;
import org.opencv.core.Size;
import org.opencv.imgproc.Imgproc;

import java.io.ByteArrayOutputStream;

/**
 * Created by lkduy on 12/12/2017.
 */

public class ImageProcessor {
    static public Bitmap getThumbnailImage(Bitmap origin){
        int scaleFactor = 4;
        Bitmap downscaledBmp = Bitmap.createScaledBitmap(origin,origin.getWidth()/scaleFactor,origin.getHeight()/scaleFactor,true);
        Mat downscaledMat = new Mat(downscaledBmp.getHeight(),downscaledBmp.getWidth(),4);
        Utils.bitmapToMat(downscaledBmp,downscaledMat);
        Imgproc.GaussianBlur(downscaledMat,downscaledMat,new Size(3,3),2);
        //Imgproc.cvtColor(downscaledMat,downscaledMat,Imgproc.COLOR_BGRA2RGB);
        Utils.matToBitmap(downscaledMat,downscaledBmp);
        return downscaledBmp;
    }
    static public Bitmap downScale(Bitmap origin, float downScaledFactor){
        int newW = (int)(origin.getWidth()*1.0f/downScaledFactor);
        int newH = (int)(origin.getHeight()*1.0f/downScaledFactor);
        return Bitmap.createScaledBitmap(origin,newW,newH,true);
    }
    static public Paint bmpPaint = null;
    static public Bitmap extractBimapPortion(Bitmap src,float percentL,float percentT, float percentW,float  percentH){
        int dstL = (int)(percentL * src.getWidth());
        int dstT = (int)(percentT * src.getHeight());
        int dstW = (int)(percentW *src.getWidth());
        int dstH = (int)(percentH * src.getHeight());
        return Bitmap.createBitmap(src,dstL,dstT,dstW,dstH);
    }
    static public Bitmap rotateBitmapQuadricAngle(Bitmap origin, double rotAngle, boolean isBmpTransparent){
        Mat src = new Mat(origin.getHeight(),origin.getWidth(),4);
        Utils.bitmapToMat(origin,src);
        Mat dst = new Mat();
        if(rotAngle == 0){
            dst = src.clone();
        }
        else if(rotAngle == 180 || rotAngle == -180) {
            Core.flip(src, dst, -1);
        } else if(rotAngle == 90 || rotAngle == -270) {
            Core.flip(src.t(), dst, 1);
        } else if(rotAngle == 270 || rotAngle == -90) {
            Core.flip(src.t(), dst, 0);
        }
        Bitmap dstBmp = getBitmapOfMat(dst,isBmpTransparent);
        return dstBmp;
    }


    static public Bitmap skewEvenlyBitmap(Bitmap srcBmp,int dstBigW,int dstSmallW,int dstH){
        float[] srcCorners = new float[]{0,0,
                                        srcBmp.getWidth(),0,
                                        srcBmp.getWidth(), srcBmp.getHeight(),
                                        0, srcBmp.getHeight()};
        float[] dstCorners = new float[]{0,0,
                                        dstBigW,0,
                                        (dstBigW + dstSmallW)/2,dstH,
                                        (dstBigW - dstSmallW)/2,dstH};
        Matrix transformMat = new Matrix();
        transformMat.setPolyToPoly(srcCorners,0,dstCorners,0,srcCorners.length/2);
        return Bitmap.createBitmap(srcBmp,0,0,srcBmp.getWidth(),srcBmp.getHeight(),transformMat,true);
    }
    static public Bitmap flipBitmap(Bitmap origin,boolean flipAroundX,boolean flipAroundY,boolean isBmpTransparent){
        Mat src = new Mat(origin.getHeight(),origin.getWidth(),4);
        Utils.bitmapToMat(origin,src);
        Mat dst = new Mat();
        int flipCode = 0;
        if(flipAroundX && flipAroundY){
            flipCode = -1;
        }
        else if(flipAroundX){
            flipCode = 0;
        }
        else{
            flipCode= 1;
        }
        Core.flip(src,dst,flipCode);
        Bitmap dstBmp = getBitmapOfMat(dst,isBmpTransparent);
        return dstBmp;
    }
    public  static Bitmap getBitmapOfMat(Mat img, boolean isTransparent){
        Bitmap bmp = null;
        try {
            if(isTransparent) {
                bmp = Bitmap.createBitmap(img.cols(), img.rows(), Bitmap.Config.ARGB_8888);
            }
            else{
                bmp = Bitmap.createBitmap(img.cols(), img.rows(), Bitmap.Config.RGB_565);
            }
            Utils.matToBitmap(img, bmp);
        } catch (CvException e) {
            Log.d("SAVING IMAGE", e.getMessage());
        }
        return bmp;
    }
    public  static byte[] getBytesFromBitmap(Bitmap bitmap, boolean preserveTransparency, int quality){
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        if(preserveTransparency){
            bitmap.compress(Bitmap.CompressFormat.PNG, quality, stream);
        }else {
            bitmap.compress(Bitmap.CompressFormat.JPEG, quality, stream);
        }
        return stream.toByteArray();
    }
}
